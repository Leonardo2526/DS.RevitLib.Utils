using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.Solids;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs.Commands
{
    /// <summary>
    /// An object that represent commands to show graph as <see cref="MEPSystem"/> in <see cref="Document"/>.
    /// </summary>
    public class ShowMEPGraphCommand : ShowGraphInDocCommandBase<IVertex>
    {
        private readonly MEPCurve _baseMEPCurve;
        private readonly MEPCurveCreator _mEPCurveCreator;

        /// <summary>
        /// Instansiate an object that represent commands to show graph as <see cref="MEPSystem"/> in <see cref="Document"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="uiDoc"></param>
        /// <param name="baseMEPCurve"></param>
        public ShowMEPGraphCommand(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            UIDocument uiDoc, MEPCurve baseMEPCurve) : base(graph, uiDoc)
        {
            _baseMEPCurve = baseMEPCurve;
            _mEPCurveCreator = new MEPCurveCreator(_baseMEPCurve);
        }

        /// <summary>
        /// Specifies whether to show vertices that aren't of <see cref="TaggedGVertex{TTag}"/> type.
        /// </summary>
        public bool ShowUntaggedVertices { get; set; }

        /// <inheritdoc/>
        public override Edge<IVertex> Show(Edge<IVertex> edge)
        {
            var v1 = edge.Source;
            var v2 = edge.Target;

            MEPCurve mEPCurve = TryCreateMEPCurve(edge, _mEPCurveCreator, _baseMEPCurve);
            return mEPCurve == null ?
                null :
                new TaggedEdge<IVertex, int>(v1, v2, mEPCurve.Id.IntegerValue);

            static MEPCurve TryCreateMEPCurve(Edge<IVertex> edge, MEPCurveCreator mEPCurveCreator, MEPCurve baseMEPCurve)
            {
                MEPCurve mEPCurve = null;
                if (edge.Source is TaggedGVertex<Point3d> sp && edge.Target is TaggedGVertex<Point3d> tp)
                { mEPCurve = mEPCurveCreator.Create(sp.Tag.ToXYZ(), tp.Tag.ToXYZ(), baseMEPCurve); }

                return mEPCurve;
            }
        }

        /// <inheritdoc/>
        public override IVertex Show(IVertex vertex)
        {
            IVertex resultvertex = null;

            if (_bdGraph.TryGetInEdges(vertex, out var inEdges))
            {
                var mEPCurve1 = TryGetMEPCurve(inEdges);
                if (mEPCurve1 != null)
                {
                    if (_bdGraph.TryGetOutEdges(vertex, out var outEdges))
                    {
                        var mEPCurve2 = TryGetMEPCurve(outEdges);
                        if (mEPCurve2 != null)
                        {
                            var famInst = FamInstCreator.CreateElbow(mEPCurve1, mEPCurve2);
                            resultvertex = new TaggedGVertex<int>(vertex.Id, famInst.Id.IntegerValue);
                        }
                    }
                }
            }

            if (ShowUntaggedVertices && resultvertex == null)
            {
                var radius = _baseMEPCurve.GetProfileType() == ConnectorProfileType.Round ?
                    _baseMEPCurve.Diameter / 2 :
                    Math.Min(_baseMEPCurve.Width, _baseMEPCurve.Height) / 2;
                XYZ pointToInsert = vertex.GetLocation(_doc);
                if (pointToInsert != null)
                {
                    var solid = new SphereCreator(radius, pointToInsert).CreateSolid();
                    var shape = solid.ShowShape(_doc);
                    resultvertex = new TaggedGVertex<int>(vertex.Id, shape.Id.IntegerValue);
                }
            }

            return resultvertex;

            MEPCurve TryGetMEPCurve(IEnumerable<Edge<IVertex>> inEdges)
            {
                MEPCurve mEPCurve = null;
                switch (inEdges.Count())
                {
                    case 1:
                        {
                            mEPCurve = inEdges.First().TryGetMEPCurve(_doc);
                            break;
                        }
                    default:
                        break;
                }
                return mEPCurve;
            }
        }

        /// <inheritdoc/>
        public override IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> ShowGraph()
        {
            var edgeGraph = ShowEdges();
            var edgeCommand = new ShowMEPGraphCommand(edgeGraph, _uiDoc, _baseMEPCurve);
            return edgeCommand.ShowVertices();
        }

        /// <inheritdoc/>
        public override async Task<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> ShowGraphAsync()
        {
            var edgeGraph = await _transactionFactory?.CreateAsync(() => ShowEdges(), "show edge");
            //return edgeGraph;
            var edgeCommand = new ShowMEPGraphCommand(edgeGraph, _uiDoc, _baseMEPCurve)
            {
                ShowUntaggedVertices = ShowUntaggedVertices,
                ShowVertexIds = ShowVertexIds,
                ShowEdgeTags = ShowEdgeTags,
                ShowVertexTags = ShowVertexTags,
                TransactionFactory = TransactionFactory
            };
            return await _transactionFactory?.CreateAsync(() => edgeCommand.ShowVertices(), "show vertices");
        }
    }
}
