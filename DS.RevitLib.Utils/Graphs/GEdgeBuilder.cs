using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Graphs;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Represents builder for graph with <see cref="GVertex"/> vertices and <see cref="Edge{TVertex}"/> edges.
    /// </summary>
    public class GEdgeBuilder : IEdgeBuilder<GVertex, Edge<GVertex>>
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
        public Edge<GVertex> GetEdge(GVertex vertex1, GVertex vertex2, int edgeTag)
            => edgeTag == 0 ? 
            new Edge<GVertex>(vertex1, vertex2) :
            new TaggedEdge<GVertex, int>(vertex1, vertex2, edgeTag);

    }
}
