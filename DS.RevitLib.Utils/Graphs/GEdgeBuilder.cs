using DS.GraphUtils.Entities;
using QuickGraph;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Represents builder for graph with <see cref="IVertex"/> vertices and <see cref="Edge{TVertex}"/> edges.
    /// </summary>
    public class GEdgeBuilder : IEdgeBuilder<IVertex, Edge<IVertex>>
    {

        /// <summary>
        /// Get edge between <paramref name="vertex1"/> and <paramref name="vertex2"/>.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="edgeTag"></param>
        /// <returns>
        /// A new edge.
        /// </returns>
        public Edge<IVertex> GetEdge(IVertex vertex1, IVertex vertex2, int edgeTag)
            => edgeTag == 0 ?
            new Edge<IVertex>(vertex1, vertex2) :
            new TaggedEdge<IVertex, int>(vertex1, vertex2, edgeTag);

    }
}
