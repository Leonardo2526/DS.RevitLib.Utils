using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Graphs;
using QuickGraph;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Represents builder for graph <typeparamref name="TVertex"/> vertices.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TTaggedVertex"></typeparam>
    public interface IVertexBuilder<TVertex, TTaggedVertex>
    {
        /// <summary>
        /// Tag value to add to incident edge.
        /// </summary>
        int EdgeTag { get; }

        /// <summary>
        /// Instansiate builder with <paramref name="open"/> stack.
        /// </summary>
        /// <param name="open"></param>
        /// <param name="graph"></param>
        void Instansiate(Stack<TTaggedVertex> open, AdjacencyGraph<TVertex, Edge<TVertex>> graph);

        /// <summary>
        /// Try to get <typeparamref name="TVertex"/> by <paramref name="vertex"/>
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        TVertex TryGetVertex(TTaggedVertex vertex);

        /// <summary>
        /// Try to get <typeparamref name="TVertex"/> by <paramref name="element"/>
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        TVertex TryGetVertex(Element element);
    }
}