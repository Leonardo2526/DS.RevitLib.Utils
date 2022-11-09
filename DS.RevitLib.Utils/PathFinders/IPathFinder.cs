using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.PathFinders
{
    public interface IPathFinder
    {
        public List<XYZ> Find(XYZ point1, XYZ point2);
    }
}
