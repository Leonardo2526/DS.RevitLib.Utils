using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.PathFinders
{
    /// <summary>
    /// Interface to get path between points.
    /// </summary>
    public interface IPathFinder
    {
        /// <summary>
        /// Find path between <paramref name="point1"/> and <paramref name="point2"/>.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns>Returns path coordinates.</returns>
        public List<XYZ> Find(XYZ point1, XYZ point2);

        /// <summary>
        /// Exception elements ids to find path.
        /// </summary>
        public List<ElementId> ExceptionElements { get; set; }
    }
}
