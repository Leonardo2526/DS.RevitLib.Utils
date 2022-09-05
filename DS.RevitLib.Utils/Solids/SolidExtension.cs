using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Extensions
{
    public static class SolidExtension
    {
        /// <summary>
        /// Get the transformed solid from the coordinate space of the box to the model coordinate space.
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        public static Solid TransformToModelSpace(this Solid solid)
        {
            Transform transform = solid.GetBoundingBox().Transform;
            return SolidUtils.CreateTransformed(solid, transform);
        }

        /// <summary>
        /// Extract all points from solid.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<XYZ> ExtractPoints(this Solid s)
        {
            List<Curve> list = (from Edge x in s.Edges
                                select x.AsCurve()).ToList();
            List<XYZ> result = new List<XYZ>();
            list.ForEach(delegate (Curve x)
            {
                result.Add(x.GetEndPoint(0));
            });
            list.ForEach(delegate (Curve x)
            {
                result.Add(x.GetEndPoint(1));
            });
            return result;
        }

        /// <summary>
        /// Get two edge points in project to line. 
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="line"></param>
        /// <returns>Get two project points with max distance between them.</returns>
        public static (XYZ point1, XYZ point2) GetEdgeProjectPoints(this Solid solid, Line line)
        {
            List<XYZ> solidPoints = solid.ExtractPoints();
            List<XYZ> projSolidPoints = solidPoints.Select(obj => line.Project(obj).XYZPoint).ToList();
            (XYZ point1, XYZ point2) = XYZUtils.GetMaxDistancePoints(projSolidPoints, out double dist);
            return (point1, point2);
        }

        /// <summary>
        /// Show bounding box of solid.
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="doc"></param>
        public static void ShowBB(this Solid solid, Document doc)
        {
            BoundingBoxXYZ box = solid.GetBoundingBox();
            IVisualisator vs = new BoundingBoxVisualisator(box, doc);
            new Visualisator(vs);
        }

    }
}
