using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="BoundingBoxXYZ"/>.
    /// </summary>
    public static class BoxXYZExtensions
    {
        /// <summary>
        /// Show <paramref name="boxXYZ"/> in document as outline model lines.
        /// </summary>
        /// <param name="boxXYZ"></param>
        /// <param name="doc"></param>
        public static void Show(this BoundingBoxXYZ boxXYZ, Document doc) =>
            new BoundingBoxVisualisator(boxXYZ, doc).Show();

        /// <summary>
        /// Get minimum and maximum coordinate points from <paramref name="boxXYZ"/> in Revit coordinates. 
        /// </summary>
        /// <param name="boxXYZ"></param>
        /// <returns>
        /// Transformed min and max points of <paramref name="boxXYZ"/> that were defined in the coordinate space of the box.
        /// </returns>
        public static (XYZ minPoint, XYZ maxPoint) GetMinMaxPoints(this BoundingBoxXYZ boxXYZ)
        {
            var transform = boxXYZ.Transform;
            XYZ minPoint = boxXYZ.Min;
            XYZ maxPoint = boxXYZ.Max;

            return (transform.OfPoint(minPoint), transform.OfPoint(maxPoint));
        }

        /// <summary>
        /// Get all corner points from <paramref name="boxXYZ"/>.
        /// </summary>
        /// <param name="boxXYZ"></param>
        /// <returns>
        /// List of corner points.
        /// </returns>
        public static List<XYZ> GetPoints(this BoundingBoxXYZ boxXYZ)
        {
            var points = new List<XYZ>();

            XYZ minPoint = boxXYZ.Min;
            XYZ maxPoint = boxXYZ.Max;

            var minPoints = GetOrthPoints(minPoint, maxPoint);
            var maxPoints = GetOrthPoints(maxPoint, minPoint);

            points.AddRange(minPoints);
            points.AddRange(maxPoints);

            return points;

            static List<XYZ> GetOrthPoints(XYZ basePoint, XYZ point)
            {
                var points = new List<XYZ>()
            {
                new XYZ(basePoint.X, basePoint.Y, basePoint.Z),
                new XYZ(point.X, basePoint.Y, basePoint.Z),
                new XYZ(basePoint.X, point.Y, basePoint.Z),
                new XYZ(basePoint.X, basePoint.Y, point.Z)
            };
                return points;
            }
        }

        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.Outline"/> from <paramref name="boxXYZ"/>.
        /// </summary>        
        /// <param name="boxXYZ"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Outline"/> built by min and max points of <paramref name="boxXYZ"/>.
        /// </returns>
        public static Outline GetOutline(this BoundingBoxXYZ boxXYZ)
        {
            var transform = boxXYZ.Transform;
            var p1 = transform.OfPoint(boxXYZ.Min);
            var p2 = transform.OfPoint(boxXYZ.Max);
            return new Outline(p1, p2);
        }

    }
}
