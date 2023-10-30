using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Graphs;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.Various;
using QuickGraph;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class MEPSystemGraphFactoryTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trfIn;
        private readonly ContextTransactionFactory _trfOut;

        public MEPSystemGraphFactoryTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = uidoc.Document;

            _trfIn = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
            _trfOut = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Outside);
            AdjacencyGraph<LVertex, Edge<LVertex>> graph = CreateGraph();


            Print(graph);

            var view = GetUIView();
            Task task = Task.Run(() => ShowAsync(graph, _doc, view, _trfOut));
        }

        public AdjacencyGraph<LVertex, Edge<LVertex>> CreateGraph()
        {
            var e1 = new ElementSelector(_uiDoc).Pick();

            var facrory = new MEPSystemGraphFactory(_doc)
            {
                TransactionFactory = _trfIn,
                UIDoc = _uiDoc
            };

            return facrory.Create(e1);
        }

        private void Print(AdjacencyGraph<LVertex, Edge<LVertex>> graph)
        {
            foreach (var vertex in graph.Vertices)
            {
                foreach (var edge in graph.OutEdges(vertex))
                {
                    var edgeTag = edge is TaggedEdge<LVertex, int> taggedEdge ? taggedEdge.Tag : 0;
                    var sTag = edge.Source is TaggedLVertex<int> taggedSource ? taggedSource.Tag : 0;
                    var tTag = edge.Target is TaggedLVertex<int> taggedTarget ? taggedTarget.Tag : 0;
                    Debug.WriteLine($"v{edge.Source.Id} elId: {sTag} -> v{edge.Target.Id} elId: {tTag}. Edge: ({edgeTag})");
                }
            }
        }

        private async void ShowAsync(AdjacencyGraph<LVertex, Edge<LVertex>> graph, Document doc, UIView view, ITransactionFactory trf)
        {
            var vector = new XYZ(0, 0, 0);

            foreach (var vertex in graph.Vertices)
            {
                foreach (var edge in graph.OutEdges(vertex))
                {
                    var v1 = edge.Source;
                    var v2 = edge.Target;
                    var edgeTag = edge is TaggedEdge<LVertex, int> taggedEdge ? taggedEdge.Tag : 0;
                    var sTag = v1 is TaggedLVertex<int> taggedSource ? taggedSource.Tag : 0;
                    var tTag = v2 is TaggedLVertex<int> taggedTarget ? taggedTarget.Tag : 0;

                    var xyz1 = v1.Location.ToXYZ();
                    var xyz2 = v2.Location.ToXYZ();

                    ElementId defaultTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);

                    xyz1.Show(doc, 0, trf);
                    if (sTag != 0)
                    {
                        await trf.CreateAsync(() =>
                       { TextNote.Create(doc, view.ViewId, xyz1 + vector, sTag.ToString(), defaultTypeId); }, "showVertexTag");
                    }

                    xyz2.Show(doc, 0, trf);
                    if (tTag != 0)
                    {
                        await trf.CreateAsync(() =>
                        { TextNote.Create(doc, view.ViewId, xyz2 + vector, tTag.ToString(), defaultTypeId); }, "showVertexTag");
                    }

                    var line = Line.CreateBound(xyz1, xyz2);
                    await trf.CreateAsync(() => line.Show(doc), "showEdge");
                    if (edgeTag != 0)
                    {
                        await trf.CreateAsync(() =>
                        { TextNote.Create(doc, view.ViewId, line.GetCenter() + vector, edgeTag.ToString(), defaultTypeId); }, "showEdgeTags");
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
    }
}
