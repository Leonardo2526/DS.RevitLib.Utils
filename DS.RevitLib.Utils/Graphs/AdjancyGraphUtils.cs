using DS.GraphUtils.Entities;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Graph useful methods.
    /// </summary>
    public static class AdjancyGraphUtils
    {
        /// <summary>
        /// Create simple chain graph from points.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> CreateSimpleChainGraph(IEnumerable<Point3d> points)        
            => new SimpleChainGraphFactory(points).CreateGraph();
        
    }
}
