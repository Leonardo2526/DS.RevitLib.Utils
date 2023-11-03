using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Graphs;
using DS.RevitLib.Utils.Extensions;
using QuickGraph;
using Rhino.Geometry;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Visualisator object to show <see cref="AdjacencyGraph{TVertex, TEdge}"/>
    /// </summary>
    public class AdjacencyGraphVisualisator : IAdjacencyGraphVisulisator<IVertex>
    {
        private readonly Document _doc;
        private AdjacencyGraph<IVertex, Edge<IVertex>> _graph;
        private readonly UIView _view;
        private readonly XYZ  _moveVector = new XYZ();

        /// <summary>
        /// Instansiate a visualisator.
        /// </summary>
        /// <param name="doc"></param>
        public AdjacencyGraphVisualisator(Document doc)
        {
            _doc = doc;
            _view = GetUIView(_doc);
        }

        /// <summary>
        /// 
        /// </summary>
        public UIDocument UiDoc { get; set; }

        /// <summary>
        /// Specified whether show <see cref="Autodesk.Revit.DB.ElementId"/>'s of verticies and edges or not.
        /// </summary>
        public bool ShowElementIds { get; set; } = false;

        /// <summary>
        /// Specified whether show verticies ids or not.
        /// </summary>
        public bool ShowVerticesIds { get; set; } = true;

        /// <inheritdoc/>
        public IAdjacencyGraphVisulisator<IVertex> Build(AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
            _graph = graph;
            return this;
        }

        /// <inheritdoc/>
        public void Show()
        {
            foreach (var vertex in _graph.Vertices)
            {
                foreach (var edge in _graph.OutEdges(vertex))
                {

                    var taggedEdge = edge as TaggedEdge<IVertex, int>;
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

                    ElementId defaultTypeId = _doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);

                    xyz1?.Show(_doc, 0);
                    if (ShowElementIds && sTag != 0)
                    { TextNote.Create(_doc, _view.ViewId, xyz1 + _moveVector, sTag.ToString(), defaultTypeId); }


                    xyz2?.Show(_doc, 0);
                    if (ShowElementIds && tTag != 0)
                    { TextNote.Create(_doc, _view.ViewId, xyz2 + _moveVector, tTag.ToString(), defaultTypeId); }

                    if(ShowVerticesIds)
                    {
                        TextNote.Create(_doc, _view.ViewId, xyz1 + _moveVector, v1.Id.ToString(), defaultTypeId);
                        TextNote.Create(_doc, _view.ViewId, xyz2 + _moveVector, v2.Id.ToString(), defaultTypeId);
                    }

                    var line = Autodesk.Revit.DB.Line.CreateBound(xyz1, xyz2);
                    line.Show(_doc);

                    if (ShowElementIds)
                    {
                        if (edgeTag != 0)
                        { TextNote.Create(_doc, _view.ViewId, line.GetCenter() + _moveVector, edgeTag.ToString(), defaultTypeId); }
                    }
                }
            }
        }

        private UIView GetUIView(Document doc)
        {
            var uidoc = new UIDocument(doc);
            var view = uidoc.ActiveGraphicalView;
            UIView uiview = null;
            var uiviews = uidoc.GetOpenUIViews();
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

        private XYZ GetLocation(int tag)
        {
            var famInst = _doc.GetElement(new ElementId(tag)) as FamilyInstance;
            return famInst.GetLocation();
        }
    }
}
