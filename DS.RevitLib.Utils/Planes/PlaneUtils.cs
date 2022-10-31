using Autodesk.Revit.DB;
using System;

namespace DS.RevitLib.Utils.Planes
{
    public static class PlaneUtils
    {
        /// <summary>
        /// Create plane by line and point. If point us null then point is assigned as one of the basis.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Plane CreateByLineAndPoint(this Line line, XYZ point = null)
        {
            XYZ p1 = line.GetEndPoint(0);
            XYZ p2 = line.GetEndPoint(1);
            point ??= GetPoint(p1, p2);

            return Plane.CreateByThreePoints(p1, p2, point);
        }

        private static XYZ GetPoint(XYZ p1, XYZ p2)
        {
            if (Math.Abs(p1.X - p2.X) < 0.01 & Math.Abs(p1.Y - p2.Y) < 0.01)
            {
                return p2 + XYZ.BasisY;
            }

            return p2 + XYZ.BasisZ;
        }
    }
}
