using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Graphs;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Various;
using QuickGraph;
using QuickGraph.Algorithms;
using Rhino.Geometry;
using Rhino.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Line = Autodesk.Revit.DB.Line;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class BestMEPSystemGraphFactoryTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trfIn;
        private readonly ContextTransactionFactory _trfOut;

        public BestMEPSystemGraphFactoryTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = uidoc.Document;

            _trfIn = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
            _trfOut = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Outside);
            AdjacencyGraph<GVertex, Edge<GVertex>> graph = CreateGraph();

            Print(graph);
            Show(graph, _doc, _trfOut);
        }

        public AdjacencyGraph<GVertex, Edge<GVertex>> CreateGraph()
        {
            var e1 = new ElementSelector(_uiDoc).Pick();

            var vertexBuilder = new GVertexBuilder(_doc);
            var edgeBuilder = new GEdgeBuilder();
            var facrory = new BestMEPSystemGraphFactory(_doc, vertexBuilder, edgeBuilder)
            {
                TransactionFactory = _trfIn,
                UIDoc = _uiDoc
            };

            return facrory.Create(e1);
        }

        private void Print(AdjacencyGraph<GVertex, Edge<GVertex>> graph)
        {
            foreach (var vertex in graph.Vertices)
            {
                foreach (var edge in graph.OutEdges(vertex))
                {
                    var edgeTag = edge is TaggedEdge<GVertex, int> taggedEdge ? taggedEdge.Tag : 0;
                    var sTag = edge.Source is TaggedGVertex<int> taggedSource ? taggedSource.Tag : 0;
                    var tTag = edge.Target is TaggedGVertex<int> taggedTarget ? taggedTarget.Tag : 0;
                    Debug.WriteLine($"v{edge.Source.Id} elId: {sTag} -> v{edge.Target.Id} elId: {tTag}. Edge: ({edgeTag})");
                }
            }

            Debug.WriteLine("Verices count is: " + graph.Vertices.Count());
            Debug.WriteLine("Edges count is: " + graph.Edges.Count());

            var emptyTagVertices = graph.Vertices.Where(v => v is not TaggedGVertex<int>).ToList();
            var taggedVertices = graph.Vertices.OfType<TaggedGVertex<int>>();
            var distinctVericies = taggedVertices.Distinct(new CompareGTaggedVertex()).ToList();
            var duplicateVertices = taggedVertices.Where(v => !distinctVericies.Contains(v)).ToList();

            Debug.WriteLine("Duplicate vertices count is: " + duplicateVertices.Count);
            duplicateVertices.ForEach(v => Debug.WriteLine(v.Id));

            //Debug.WriteLine("Empty verices count is: " + emptyTagVertices.Count);
            //emptyTagVertices.ForEach(v => Debug.WriteLine(v.Id));


            var taggedEdges = graph.Edges.OfType<TaggedEdge<GVertex, int>>();
            var distinctsEdges = taggedEdges.Distinct(new CompareGTaggedEdge()).ToList();
            var duplicateEdges = taggedEdges.Where(e => !distinctsEdges.Contains(e)).ToList();
            Debug.WriteLine("Duplicate edges count is: " + duplicateEdges.Count);
            duplicateEdges.ForEach(v => Debug.WriteLine(v.Tag));
        }

        private void Show(AdjacencyGraph<GVertex, Edge<GVertex>> graph, Document doc, ITransactionFactory trf)
        {
            var view = GetUIView();

            Task task = Task.Run(async () =>
            await trf.CreateAsync(() => ShowGraph(graph, doc, view),
            "show"));

            void ShowGraph(AdjacencyGraph<GVertex, Edge<GVertex>> graph, Document doc, UIView view)
            {
                var vector = new XYZ(0, 0, 0);

                bool printElementIds = true;

                foreach (var vertex in graph.Vertices)
                {
                    foreach (var edge in graph.OutEdges(vertex))
                    {

                        var taggedEdge = edge as TaggedEdge<GVertex, int>;
                        var mEPCurve = taggedEdge is not null ? _doc.GetElement(new ElementId(taggedEdge.Tag)) : null;

                        var v1 = edge.Source;
                        var v2 = edge.Target;
                        var edgeTag = taggedEdge?.Tag;
                        var sTag = v1 is TaggedGVertex<int> taggedSource ? taggedSource.Tag : 0;
                        var tTag = v2 is TaggedGVertex<int> taggedTarget ? taggedTarget.Tag : 0;

                        var defaultPoint = new Point3d(double.NaN, double.NaN, double.NaN);
                        var slTag = v1 is TaggedGVertex<Point3d> ltaggedSource ? ltaggedSource.Tag : defaultPoint;
                        var tlTag = v2 is TaggedGVertex<Point3d> ttaggedSource ? ttaggedSource.Tag : defaultPoint;

                        var xyz1 = sTag == 0 ? slTag.ToXYZ() : GetLocation(sTag);
                        var xyz2 = tTag == 0 ? tlTag.ToXYZ() : GetLocation(tTag);

                        ElementId defaultTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);

                        var txt1 = printElementIds ? sTag : v1.Id;
                        var txt2 = printElementIds ? tTag : v2.Id;

                        xyz1?.Show(doc, 0);
                        if (!printElementIds || sTag != 0)
                        { TextNote.Create(doc, view.ViewId, xyz1 + vector, txt1.ToString(), defaultTypeId); }

                        xyz2?.Show(doc, 0);
                        if (!printElementIds || tTag != 0)
                        { TextNote.Create(doc, view.ViewId, xyz2 + vector, txt2.ToString(), defaultTypeId); }

                        var line = Line.CreateBound(xyz1, xyz2);
                        line.Show(doc);

                        if (printElementIds)
                        {
                            if (edgeTag != 0)
                            { TextNote.Create(doc, view.ViewId, line.GetCenter() + vector, edgeTag.ToString(), defaultTypeId); }
                        }
                    }
                }

            }
        }


        private UIView GetUIView()
        {
            var uidoc = new UIDocument(_doc);
            var view = uidoc.ActiveGraphicalView;
            UIView uiview = null;
            var uiviews = _uiDoc.GetOpenUIViews();
            foreach (UIView uv in uiviews)
            {
                if (uv.ViewId.Equals(view.Id))
                {
                    uiview = uv;
                    break;
                }
            }
            return uiview;
        }

        private XYZ GetLocation(TaggedGVertex<int> vertex)
        {
            var famInst = _doc.GetElement(new ElementId(vertex.Tag)) as FamilyInstance;
            return famInst.GetLocationPoint();
        }

        private XYZ GetLocation(int tag)
        {
            var famInst = _doc.GetElement(new ElementId(tag)) as FamilyInstance;
            return famInst.GetLocation();
        }

    }
}
