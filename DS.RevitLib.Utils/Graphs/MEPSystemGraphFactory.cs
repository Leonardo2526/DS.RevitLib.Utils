using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Graphs;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP;
using MoreLinq.Extensions;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{
    public class MEPSystemGraphFactory : MEPSystemGraphFactoryBase<AdjacencyGraph<LVertex, Edge<LVertex>>>
    {

        public MEPSystemGraphFactory(Document doc) : base(doc)
        {

        }

        public UIDocument UIDoc { get; set; }
        public ITransactionFactory TransactionFactory { get; set; }

        public override AdjacencyGraph<LVertex, Edge<LVertex>> Create(Element element)
        {
            var open = new Stack<(LVertex parent, IEnumerable<Element> childElements)>();

            var connectedElements = element.GetConnected();
            if (connectedElements.Count == 0)
            {
                FillEmpty(element);
                return Graph;
            }

            var firstVertex = GetFirstVertex(element);
            if(element is MEPCurve mEPCurve) 
            {
                var points = GetOrderedPoints(connectedElements, mEPCurve, firstVertex);
                connectedElements.Clear();
                points.ForEach(p => connectedElements.Add(_doc.GetElement(p.id)));
            }

            open.Push((firstVertex, connectedElements));

            while (open.Count > 0)
            {
                var current = open.Pop();

                //Show vertex
                TransactionFactory?.CreateAsync(() =>
                {
                    if (current.parent is TaggedLVertex<int> taggedLVertex)
                    {
                        var elemId = new ElementId(taggedLVertex.Tag);
                        UIDoc?.Selection.SetElementIds(new List<ElementId>() { elemId });
                    }
                }, "showCurrentVertex");

                var parentVertex = current.parent;
                var childElements = current.childElements;

                foreach (var childElement in childElements)
                {
                    var edgeTag = childElement is MEPCurve ? childElement.Id.IntegerValue : 0;

                    //add to graph
                    IEnumerable<LVertex> childVertices = GetVertices(childElement, parentVertex);

                    var verticesSet = new List<LVertex>() { parentVertex };
                    verticesSet.AddRange(childVertices);

                    for (int i = 0; i < verticesSet.Count - 1; i++)
                    {
                        var v1 = verticesSet[i];
                        var v2 = verticesSet[i + 1];
                        var edge = edgeTag == 0 ?
                            new Edge<LVertex>(v1, v2) :
                            new TaggedEdge<LVertex, int>(v1, v2, edgeTag);
                        Graph.AddEdge(edge);
                    }

                    //try push to open
                    var taggedChildVertices = childVertices.OfType<TaggedLVertex<int>>().ToList();
                    foreach (var taggedChildVertex in taggedChildVertices)
                    {

                        var chElement = _doc.GetElement(new ElementId(taggedChildVertex.Tag));
                        ElementId excludeElementId = GetExcluded(parentVertex, taggedChildVertex);
                        var cvConnected = chElement.GetConnected().Where(e => e.Id != excludeElementId);
                        var item = (taggedChildVertex, cvConnected);
                        open.Push(item);
                    }

                }

            }

            return Graph;

            void FillEmpty(Element element)
            {
                switch (element)
                {
                    case MEPCurve mEPCurve:
                        {
                            TaggedEdge<LVertex, int> edge = CreateEdge(mEPCurve);
                            Graph.AddEdge(edge);
                        }
                        break;
                    case FamilyInstance familyInstance:
                        {
                            var vertex = CreateVertex(0, familyInstance);
                            Graph.AddVertex(vertex);
                        }
                        break;
                    default: throw new NotImplementedException();
                }
            }
        }

        private ElementId GetExcluded(LVertex parentVertex, TaggedLVertex<int> taggedChildVertex)
        {
            Graph.TryGetEdge(parentVertex, taggedChildVertex, out Edge<LVertex> edge);

            if(edge is TaggedEdge<LVertex, int> taggedEdge) 
            { return new ElementId(taggedEdge.Tag); }

            if(parentVertex is TaggedLVertex<int> taggedParentVertex)
            { return new ElementId(taggedParentVertex.Tag); }

            return new ElementId(0);
        }

        private LVertex GetFirstVertex(Element element)
        {
            LVertex vertex;

            switch (element)
            {
                case MEPCurve mEPCurve:
                    {
                        var freeCons = ConnectorUtils.GetFreeConnector(mEPCurve);
                        if (freeCons is not null && freeCons.Count > 0)
                        {
                            var location = freeCons.FirstOrDefault().Origin.ToPoint3d();
                            vertex = new LVertex(0, location);
                        }
                        else
                        {
                            var connected = mEPCurve.GetConnected();
                            var familyInstance = connected.FirstOrDefault();
                            var location = familyInstance.GetCenterPoint().ToPoint3d();
                            vertex = new TaggedLVertex<int>(0, location, familyInstance.Id.IntegerValue);
                        }
                    }
                    break;
                case FamilyInstance familyInstance:
                    {
                        vertex = CreateVertex(0, familyInstance);
                    }
                    break;
                default: throw new NotImplementedException();
            }

            return vertex;
        }

        /// <summary>
        /// Get ordered by <paramref name="parentVertex"/> vertices
        /// </summary>
        /// <param name="childElement"></param>
        /// <param name="parentVertex"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private IEnumerable<LVertex> GetVertices(Element childElement, LVertex parentVertex)
        {
            var vertices = new List<LVertex>();

            var currentId = Graph.Vertices.Count();
            switch (childElement)
            {
                case MEPCurve mEPCurve:
                    {

                        var parentElementId = parentVertex is TaggedLVertex<int> taggedLVertex ? taggedLVertex.Tag : 0;

                        var childConnected = childElement.GetConnected();
                        childConnected = parentElementId == 0 ?
                            childConnected :
                            childConnected.Where(c => c.Id.IntegerValue != parentElementId).ToList();

                        //Show connected
                        TransactionFactory?.CreateAsync(() =>
                        {
                            var elementsIds = childConnected.Select(c => c.Id).ToList();
                            UIDoc?.Selection.SetElementIds(elementsIds);
                        }, "ShowConnected");

                        List<(ElementId id, Point3d location)> list = GetOrderedPoints(childConnected, mEPCurve, parentVertex);

                        foreach (var item in list)
                        {
                            var vertex = item.id == null ?
                                new LVertex(currentId++, item.location) :
                                new TaggedLVertex<int>(currentId++, item.location, item.id.IntegerValue);
                            vertices.Add(vertex);
                        }
                    }
                    break;
                case FamilyInstance familyInstance:
                    {                       
                        var vertex = CreateVertex(currentId++, familyInstance);
                        vertices.Add(vertex);
                    }
                    break;
                default: throw new NotImplementedException();
            }


            return vertices;
        }

        private List<(ElementId id, Point3d location)> GetOrderedPoints(List<Element> connectedElements, MEPCurve mEPCurve, LVertex parentVertex)
        {
            var line = mEPCurve.GetCenterLine();
            var mainCons = mEPCurve.GetMainConnectors();
            var freeCons = ConnectorUtils.GetFreeConnector(mEPCurve);


            var list = new List<(ElementId id, Point3d location)>();
            freeCons.ForEach(c => list.Add((null, c.Origin.ToPoint3d())));

            foreach (var elem in connectedElements)
            {
                var c = elem.GetCenterPoint();
                var cproj = line.Project(c).XYZPoint;
                var pointToAdd = cproj.OnLine(line, false) ? cproj : c;
                list.Add((elem.Id, pointToAdd.ToPoint3d()));
            }
            var refPoint = parentVertex.Location;
            list = list.OrderBy(l => l.location.DistanceTo(refPoint)).ToList();
            return list;
        }

        public override AdjacencyGraph<LVertex, Edge<LVertex>> Create(Element element1, Element element2)
        {
            throw new NotImplementedException();
        }

        private TaggedEdge<LVertex, int> CreateEdge(MEPCurve mEPCurve)
        {
            var freeCons = ConnectorUtils.GetFreeConnector(mEPCurve);
            var v1 = new LVertex(0, freeCons[0].Origin.ToPoint3d());
            var v2 = new LVertex(1, freeCons[1].Origin.ToPoint3d());
            var edge = new TaggedEdge<LVertex, int>(v1, v2, mEPCurve.Id.IntegerValue);
            return edge;
        }

        private TaggedLVertex<int> CreateVertex(int vertexId, FamilyInstance familyInstance)
        {
            var location = familyInstance.GetCenterPoint().ToPoint3d();
            return new TaggedLVertex<int>(vertexId, location, familyInstance.Id.IntegerValue);
        }
    }
}
