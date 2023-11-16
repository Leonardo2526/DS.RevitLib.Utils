using DS.GraphUtils.Entities;
using QuickGraph;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// An object to iterate through verticed pairs of graph.
    /// </summary>
    public interface IVertexPairIteratorBuilder
    {
        /// <summary>
        /// Create iterator.
        /// </summary>
        /// <param name="graph"></param>
        /// <returns>Iterator to move through verices pairs.</returns>
        IEnumerator<(IVertex, IVertex)> Create(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph);
    }
}