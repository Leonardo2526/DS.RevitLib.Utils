using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Graphs;
using QuickGraph;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Extension methods for <see cref="Edge{TVertex}"/>.
    /// </summary>
    public static class EdgeExtensions
    {
        /// <summary>
        /// Get <paramref name="edge"/>'s length.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// Distance between <paramref name="edge"/>'s source and target location points.
        /// </returns>
        public static double GetLength(this Edge<IVertex> edge, Document doc)
        {
            var loc1 = edge.Source.GetLocation(doc);
            var loc2 = edge.Target.GetLocation(doc);

            return loc1.DistanceTo(loc2);
        }
    }
}
