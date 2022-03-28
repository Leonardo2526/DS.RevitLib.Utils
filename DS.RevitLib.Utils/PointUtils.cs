using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils
{
    public static class PointUtils
    {

        /// <summary>
        /// Select point from the list which is closest to base point;
        /// </summary>
        /// <param name="basePoint"></param>
        /// <param name="points"></param>
        /// <returns>Return closest point.</returns>
        public static XYZ SelectClosestToPoint(XYZ basePoint, List<XYZ> points)
        {
            XYZ point = points.FirstOrDefault();
            double distance = basePoint.DistanceTo(point);

            if (points.Count > 1)
            {
                for (int i = 1; i < points.Count; i++)
                {
                    double curDistance = basePoint.DistanceTo(points[i]);
                    if (curDistance < distance)
                    {
                        distance = curDistance;
                        point = points[i];
                    }
                }
            }

            return point;
        }
    }
}
