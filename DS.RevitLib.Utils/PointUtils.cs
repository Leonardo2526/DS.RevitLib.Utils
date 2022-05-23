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

        /// <summary>
        /// Check if point's projection to segment is between start and end poins.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="point"></param>
        /// <returns>Return true if point's projection lies between points of segment.</returns>
        public static bool IsPointProjInRange(XYZ startPoint, XYZ endPoint, XYZ point)
        {
            double dx = endPoint.X - startPoint.X;
            double dy = endPoint.Y - startPoint.Y;
            double dz = endPoint.Z - startPoint.Z;
            double innerProduct = (point.X - startPoint.X) * dx + (point.Y - startPoint.Y) * dy + (point.Z - startPoint.Z) * dz;
            return 0 <= innerProduct && innerProduct <= dx * dx + dy * dy + dz * dz;
        }


        /// <summary>
        /// Get min and max points whish distance to line by perpendicular if max and min.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="line"></param>
        /// <returns>Return max and min points.</returns>
        public static (XYZ minPoint, XYZ maxPoint) GetMinMaxPoints(List<XYZ> points, Line line)
        {
            double maxDist = line.Distance(points.First());
            XYZ maxPoint = points.First();

            double minDist = line.Distance(points.First());
            XYZ minPoint = points.First();

            foreach (var point in points)
            {
                if (line.Distance(point) > maxDist)
                {
                    maxDist = line.Distance(point);
                    maxPoint = point;
                }
                else if (line.Distance(point) < minDist)
                {
                    minDist = line.Distance(point);
                    minPoint = point;
                }
            }

            return (minPoint, maxPoint);
        }

    }
}
