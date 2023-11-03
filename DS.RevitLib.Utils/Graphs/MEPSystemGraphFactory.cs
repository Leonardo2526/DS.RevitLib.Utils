using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Graphs;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using MoreLinq.Extensions;
using QuickGraph;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Graphs
{
    /// <inheritdoc/>
    public class MEPSystemGraphFactory : MEPSystemGraphFactoryBase<AdjacencyGraph<IVertex, Edge<IVertex>>>
    {
        private readonly IVertexBuilder<IVertex, TaggedGVertex<int>> _vertexBuilder;
        private readonly IEdgeBuilder<IVertex, Edge<IVertex>> _edgeBuilder;

        /// <inheritdoc/>
        public MEPSystemGraphFactory(Document doc,
            IVertexBuilder<IVertex, TaggedGVertex<int>> vertexBuilder,
            IEdgeBuilder<IVertex, Edge<IVertex>> edgeBuilder) : base(doc)
        {
            _vertexBuilder = vertexBuilder;
            _edgeBuilder = edgeBuilder;
        }

        /// <summary>
        /// 
        /// </summary>
        public UIDocument UIDoc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ITransactionFactory TransactionFactory { get; set; }


        /// <inheritdoc/>
        public override AdjacencyGraph<IVertex, Edge<IVertex>> Create(Element element)
        {
            _graph = new AdjacencyGraph<IVertex, Edge<IVertex>>();

            var open = new Stack<TaggedGVertex<int>>();
            _vertexBuilder.Instansiate(_graph);

            var gBuilder = _vertexBuilder as GVertexBuilder;
            var firstTaggedVertex = gBuilder?.TryGetVertex(element) is TaggedGVertex<int> firstTagged ?
                firstTagged : default;
            if (firstTaggedVertex.Tag == 0)
            { return _graph = CreateUntaggedGraph(element as MEPCurve); }

            _graph.AddVertex(firstTaggedVertex);

            open.Push(firstTaggedVertex);
            while (open.Count > 0)
            {
                var v1 = open.Pop();
                //if (v1.Tag == 714760)
                //{ }
                while (true)
                {
                    var v2 = _vertexBuilder.TryGetVertex(v1);
                    if (v2 == null) { break; }

                    if (v2 is TaggedGVertex<int> tagged)
                    {
                        var foundInOpen = open.FirstOrDefault(v => v.Tag == tagged.Tag);
                        if (foundInOpen.Tag > 0)
                        { v2 = foundInOpen; }
                        else
                        { open.Push(tagged); }
                    };

                    _graph.AddVertex(v2);

                    var edge = _edgeBuilder.GetEdge(v1, v2, _vertexBuilder.EdgeTag);
                    _graph.AddEdge(edge);
                }
            }

            return _graph;
        }

        private AdjacencyGraph<IVertex, Edge<IVertex>> CreateUntaggedGraph(MEPCurve mEPCurve)
        {
            var untagged = new AdjacencyGraph<IVertex, Edge<IVertex>>();
            var freeCons = ConnectorUtils.GetFreeConnector(mEPCurve);

            var v1 = new TaggedGVertex<Point3d>(0, freeCons[0].Origin.ToPoint3d());
            var v2 = new TaggedGVertex<Point3d>(1, freeCons[1].Origin.ToPoint3d());

            untagged.AddVertex(v1);
            untagged.AddVertex(v2);

            var edge = _edgeBuilder.GetEdge(v1, v2, mEPCurve.Id.IntegerValue);
            untagged.AddEdge(edge);

            return untagged;
        }
    }
}
