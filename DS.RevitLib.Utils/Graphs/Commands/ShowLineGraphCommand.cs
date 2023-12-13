using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry.Points;
using QuickGraph;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs.Commands
{
    /// <summary>
    /// An object that represent commands to show graph as <see cref="ModelCurve"/>s in <see cref="Document"/>.
    /// </summary>
    public class ShowLineGraphCommand : ShowGraphInDocCommandBase<IVertex>
    {
        private ElementId _defaultTypeId;
        private double _labelSize = 100.MMToFeet();
        private XYZVisualizator _xYZVisulalizator;
        private readonly UIView _view;
        private readonly XYZ _moveVector = new();

        /// <summary>
        /// Instansiate an object that represent commands to show graph as <see cref="ModelCurve"/>s in <see cref="Document"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="uiDoc"></param>
        public ShowLineGraphCommand(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, UIDocument uiDoc) :
            base(graph, uiDoc)
        {
            _view = _doc.GetUIView();
            _defaultTypeId = _doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
        }

        /// <summary>
        /// Instansiate an object that represent commands to show graph as <see cref="ModelCurve"/>s in <see cref="Document"/>.
        /// </summary>
        /// <param name="uiDoc"></param>
        public ShowLineGraphCommand(UIDocument uiDoc) :
            base(uiDoc)
        {
            _view = _doc.GetUIView();
            _defaultTypeId = _doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
        }

        /// <summary>
        /// Labels size.
        /// </summary>
        public double LabelSize
        { get => _labelSize; set => _labelSize = value; }

        /// <summary>
        /// Specified whether show edges directions or not.
        /// </summary>
        public bool ShowDirecionts { get; set; } = true;

        /// <inheritdoc/>
        public override IVertex Show(IVertex vertex)
        {
            XYZ xYZPoint = vertex.GetLocation(_doc);
            var curves = xYZPoint?.ShowPoint(_doc, _labelSize);
            var ids = curves.Select(c => c.Id.IntegerValue).ToList();
            if (ids is null) { return null; }


            if (ShowVertexIds)
            {
                var n = TextNote.Create(_doc, _view.ViewId, xYZPoint + _moveVector, vertex.Id.ToString(), _defaultTypeId);
                ids.Add(n.Id.IntegerValue);
            }
            if (ShowVertexTags)
            {
                if (vertex is TaggedGVertex<Point3d> taggedPoint)
                {
                    var n = TextNote.Create(_doc, _view.ViewId, xYZPoint + _moveVector, taggedPoint.Tag.ToString(), _defaultTypeId);
                    ids.Add(n.Id.IntegerValue);
                }
            }

            return new TaggedGVertex<IEnumerable<int>>(vertex.Id, ids);
        }

        /// <inheritdoc/>
        public override Edge<IVertex> Show(Edge<IVertex> edge)
        {
            var v1 = edge.Source;
            var v2 = edge.Target;
            var xyz1 = v1.GetLocation(_doc);
            var xyz2 = v2.GetLocation(_doc);
            var line = Autodesk.Revit.DB.Line.CreateBound(xyz1, xyz2);

            var curves = new List<ModelCurve>();
            if (ShowDirecionts)
            {
                _xYZVisulalizator ??= new XYZVisualizator(_uiDoc);
                var directions = _xYZVisulalizator.ShowVectorWithoutTransaction(xyz1, xyz2);
                if (directions is not null)
                { curves.AddRange(directions); }
            }
            else
            {
                var curve = line.Show(_doc);
                if (curve is not null)
                { curves.Add(curve); }
            }

            var ids = curves.Select(c => c.Id.IntegerValue).ToList();
            if (ShowEdgeTags)
            {
                var taggedEdge = edge as TaggedEdge<IVertex, int>;
                var edgeTag = taggedEdge?.Tag;
                if (edgeTag != null)
                {
                    var xYZPoint = line.GetCenter();
                    var n = TextNote.Create(_doc, _view.ViewId, xYZPoint + _moveVector, edgeTag.ToString(), _defaultTypeId);
                    ids.Add(n.Id.IntegerValue);
                }
            }

            return new TaggedEdge<IVertex, IEnumerable<int>>(edge.Source, edge.Target, ids);
        }

        /// <inheritdoc/>
        public override IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> ShowGraph()
        {
            var edgeGraph = ShowEdges();
            var vertexCommand = GetVertexCommand(edgeGraph);
            return vertexCommand.ShowVertices();
        }

        /// <inheritdoc/>
        public override async Task<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> ShowGraphAsync()
        {
            var edgeGraph = await _transactionFactory?.CreateAsync(() => ShowEdges(), "show edge");
            //return edgeGraph;
            var vertexCommand = GetVertexCommand(edgeGraph);
            return await _transactionFactory?.CreateAsync(() => vertexCommand.ShowVertices(), "show vertices");
        }

        private ShowLineGraphCommand GetVertexCommand(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> edgeGraph)
        {
            return new ShowLineGraphCommand(edgeGraph, _uiDoc)
            {
                LabelSize = LabelSize,
                ShowDirecionts = ShowDirecionts,
                ShowEdgeTags = ShowEdgeTags,
                ShowVertexIds = ShowVertexIds,
                ShowVertexTags = ShowVertexTags,
                TransactionFactory = TransactionFactory
            };
        }
    }
}
