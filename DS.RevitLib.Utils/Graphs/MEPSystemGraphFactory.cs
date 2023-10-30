using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Graphs;
using DS.ClassLib.VarUtils.GridMap;
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
using System.Windows;
using System.Xml.Linq;

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
            _graph = new AdjacencyGraph<LVertex, Edge<LVertex>>();

            var open = new Stack<(LVertex parent, IEnumerable<Element> childElements)>();

            var firstVertex = GetFirstVertex(element);
            var connectedElements = GetConnected(firstVertex, element);

            _graph.AddVertex(firstVertex);
            open.Push((firstVertex, connectedElements));

            while (open.Count > 0)
            {
                var current = open.Pop();
                //Show(current);

                var parentVertex = current.parent;
                var childElements = current.childElements;

                foreach (var childElement in childElements)
                {
                    var edgeTag = childElement is MEPCurve ? childElement.Id.IntegerValue : 0;

                    //add to graph
                    var childVertices = GetVertices(childElement, parentVertex).ToList();

                    for (int i = 0; i < childVertices.Count; i++)
                    {
                        var v1 = i == 0 ? parentVertex : childVertices[i - 1];
                        var v2 = childVertices[i];
                        _graph.AddVertex(v2);

                        var edge = edgeTag == 0 ?
                            new Edge<LVertex>(v1, v2) :
                            new TaggedEdge<LVertex, int>(v1, v2, edgeTag);
                        _graph.AddEdge(edge);
                    }
                    //return _graph;
                    //try push to open
                    var taggedChildVertices = childVertices.OfType<TaggedLVertex<int>>().ToList();
                    ElementId excludeElementId = GetExcluded(parentVertex, edgeTag);
                    foreach (var taggedChildVertex in taggedChildVertices)
                    {
                        var chElement = _doc.GetElement(new ElementId(taggedChildVertex.Tag));
                        var cvConnected = ConnectorUtils.GetConnectedSuperbElements(chElement).
                            Where(e => e.Id != excludeElementId);
                        if (cvConnected.Any())
                        {
                            var item = (taggedChildVertex, cvConnected);
                            open.Push(item);
                        }
                    }

                }

            }

            return _graph;
        }


        private List<Element> GetConnected(LVertex firstVertex, Element element)
        {
            var list = new List<Element>();
            if (firstVertex is TaggedLVertex<int> taggedLVertex)
            {
                var vertexElement = _doc.GetElement(new ElementId(taggedLVertex.Tag));
                list = ConnectorUtils.GetConnectedElements1(vertexElement, true);
            }
            else
            {
                list.Add(element);
            }

            return list;
        }

        private ElementId GetExcluded(LVertex parentVertex, int edgeTag)
        {
            if (edgeTag != 0)
            { return new ElementId(edgeTag); }

            if (parentVertex is TaggedLVertex<int> taggedParentVertex)
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

            var currentId = _graph.Vertices.Count();


            switch (childElement)
            {
                case MEPCurve mEPCurve:
                    {
                        //Show(childConnected);
                        var parentElementId = parentVertex is TaggedLVertex<int> taggedLVertex ? taggedLVertex.Tag : 0;
                        var childConnected = ConnectorUtils.GetConnectedSuperbElements(childElement).Cast<FamilyInstance>();
                        childConnected = parentElementId == 0 ?
                            childConnected :
                            childConnected.Where(c => c.Id.IntegerValue != parentElementId).ToList();

                        List<(ElementId id, Point3d location)> list = GetOrderedPoints(childConnected, mEPCurve, parentVertex);

                        foreach (var item in list)
                        {
                            var vertex = item.id == null ?
                                new LVertex(currentId, item.location) :
                                new TaggedLVertex<int>(currentId, item.location, item.id.IntegerValue);
                            vertices.Add(vertex);
                            currentId++;
                        }
                    }
                    break;
                case FamilyInstance familyInstance:
                    {
                        var vertex = CreateVertex(currentId, familyInstance);
                        vertices.Add(vertex);
                    }
                    break;
                default: throw new NotImplementedException();
            }


            return vertices;
        }

        private List<(ElementId id, Point3d location)> GetOrderedPoints(IEnumerable<FamilyInstance> connectedFamInst, MEPCurve mEPCurve, LVertex parentVertex)
        {
            var line = mEPCurve.GetCenterLine();
            var mainCons = mEPCurve.GetMainConnectors();
            var freeCons = ConnectorUtils.GetFreeConnector(mEPCurve).
                Where(c => c.Origin.ToPoint3d().DistanceTo(parentVertex.Location) > 0.001);


            var list = new List<(ElementId id, Point3d location)>();
            freeCons.ForEach(c => list.Add((null, c.Origin.ToPoint3d())));

            foreach (var famInst in connectedFamInst)
            {              
                var pointToAdd = GetLocation(famInst);
                list.Add((famInst.Id, pointToAdd));
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
            var point = GetLocation(familyInstance);
            return new TaggedLVertex<int>(vertexId, point, familyInstance.Id.IntegerValue);
        }

        private static Point3d GetLocation(FamilyInstance familyInstance)
        {
            var location = familyInstance.GetCenterPoint();

            XYZ point;
            if (familyInstance.IsElbow())
            {
                point = location;
            }
            else if (familyInstance.IsSpud())
            {
                (List<Element> parents, Element child) = familyInstance.GetConnectedElements(true);
                var parent = parents.FirstOrDefault();
                var mainLine = parent == null ? familyInstance.GetCenterLine() : parent.GetCenterLine();
                var lineOnProject = mainLine.IncreaseLength(100);
                point = lineOnProject.Project(location).XYZPoint;
            }
            else
            {
                var mainLine = familyInstance.GetCenterLine();
                var lineOnProject = mainLine.IncreaseLength(100);
                point = lineOnProject.Project(location).XYZPoint;
            }

            return point.ToPoint3d();
        }

        //Show vertex
        private void Show((LVertex parent, IEnumerable<Element> childElements) current)
        {
            TransactionFactory?.CreateAsync(() =>
            {
                if (current.parent is TaggedLVertex<int> taggedLVertex)
                {
                    var elemId = new ElementId(taggedLVertex.Tag);
                    UIDoc?.Selection.SetElementIds(new List<ElementId>() { elemId });
                }
            }, "showCurrentVertex");
        }

        //Show elements
        private void Show(List<Element> elements)
        {
            TransactionFactory?.CreateAsync(() =>
            {
                var elementsIds = elements.Select(c => c.Id).ToList();
                UIDoc?.Selection.SetElementIds(elementsIds);
            }, "ShowConnected");
        }
    }
}
