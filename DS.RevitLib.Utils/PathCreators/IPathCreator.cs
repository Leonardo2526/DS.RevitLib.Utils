using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.PathCreators
{
    /// <summary>
    /// The interface used to create path between points.
    /// </summary>
    public interface IPathCreator
    {
        /// <summary>
        /// Create path between <paramref name="point1"/> and <paramref name="point2"/>.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns>Returns path coordinates.</returns>
        public Task<List<XYZ>> CreateAsync(XYZ point1, XYZ point2);
    }
}
