using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.Visualisators;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Extension methods for 'Solid' object.
    /// </summary>
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

        /// <summary>
        /// Get all EdgeArrays from solid.
        /// </summary>
        /// <param name="solid"></param>
        /// <returns>Returns all EdgeArrays of <paramref name="solid"/>.</returns>
        public static List<EdgeArray> GetEdgeArrays(this Solid solid)
        {
            var faces = solid.Faces;

            var edgeArrays = new List<EdgeArray>();
            foreach (Face face in faces)
            {
                for (int i = 0; i < face.EdgeLoops.Size; i++)
                {
                    EdgeArray edgeArray = face.EdgeLoops.get_Item(i);
                    edgeArrays.Add(edgeArray);
                }
            }
            return edgeArrays;
        }

        /// <summary>
        /// Get all curves from solid.
        /// </summary>
        /// <param name="solid"></param>
        /// <returns>Returns all curves from edges of <paramref name="solid"/>.</returns>
        public static List<Curve> GetCurves(this Solid solid)
        {
            List<EdgeArray> edgeArrays = solid.GetEdgeArrays();

            var curves = new List<Curve>();
            foreach (EdgeArray edgeArray in edgeArrays)
            {
                for (int i = 0; i < edgeArray.Size; i++)
                {
                    Edge edge = edgeArray.get_Item(i);
                    var curve = edge.AsCurve();
                    curves.Add(curve);
                }
            }

            return curves;
        }

        /// <summary>
        /// Show all edges of solid.
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="doc"></param>
        /// <remarks>Transaction is not provided, so methods should be wrapped to transacion.</remarks>
        public static void ShowEdges(this Solid solid, Document doc)
        {
            var curves = solid.GetCurves();
            curves.ForEach(obj => obj.Show(doc));
        }

        /// <summary>
        /// Show shape of <paramref name="solid"/>.
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="doc"></param>
        public static DirectShape ShowShape(this Solid solid, Document doc)
        {
            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
            ds.SetShape(new GeometryObject[] { solid });           

            return ds;
        }

        /// <summary>
        /// Check if <paramref name="solid"/> contains <paramref name="point"/>.
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="point"></param>
        /// <returns>Returns true if <paramref name="point"/> is inside <paramref name="solid"/>.</returns>
        public static bool Contains(this Solid solid, XYZ point)
        {
            double multiplicator = 100;
            Line line1 = Line.CreateBound(point, point + XYZUtils.GenerateXYZ().Multiply(multiplicator));

            var faces = solid.Faces;
            int intersectionCount = 0;
            foreach (Face face in faces)
            {
                if (face.Intersect(line1) == SetComparisonResult.Overlap)
                { intersectionCount++; }
            }

            return intersectionCount % 2 != 0;
        }
    }
}
