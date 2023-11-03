using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Graphs;
using QuickGraph;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Represents builder for graph with <typeparamref name="TVertex"/> vertices and <typeparamref name="TEdge"/> edges.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TEdge"></typeparam>
    public interface IEdgeBuilder<TVertex, TEdge>
    {
        /// <summary>
        /// Get <typeparamref name="TEdge"/> between <paramref name="vertex1"/> and <paramref name="vertex2"/>.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="edgeTag"></param>
        /// <returns>
        /// A new <typeparamref name="TEdge"/>.
        /// </returns>
        TEdge GetEdge(TVertex vertex1, TVertex vertex2, int edgeTag);
    }
}