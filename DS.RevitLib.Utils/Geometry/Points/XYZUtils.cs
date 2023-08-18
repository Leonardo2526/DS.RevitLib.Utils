using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry.Points;
using DS.RevitLib.Utils.Points.XYZAlgorithms.MaxDistance;
using DS.RevitLib.Utils.Points.XYZAlgorithms.Strategies;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Line = Autodesk.Revit.DB.Line;
using Matrix = DS.ClassLib.VarUtils.Matrix;

namespace DS.RevitLib.Utils
{
    /// <summary>
    ///  Object representing tools to work with XYZ points.
    /// </summary>
    public static class XYZUtils
    {
        #region PublicMethods

        /// <summary>
        /// Check if three vectors system have orientation (left or right) like origin
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns>Return true if three vectors system have orientation like origin.</returns>
        public static bool BasisEqualToOrigin(XYZ a, XYZ b, XYZ c)
        {
            a = a.RoundVector();
            b = b.RoundVector();
            c = c.RoundVector();

            double[,] matrix = CreateMatrix3D(a, b, c);
            double det = Matrix.GetMatrixDeterminant(matrix);

            if (det > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if two vectors are colliner.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Return true if two vectors are colliner.</returns>
        public static bool Collinearity(XYZ a, XYZ b)
        {
            a = a.RoundVector();
            b = b.RoundVector();

            XYZ crossProduct = a.CrossProduct(b);
            if (Math.Round(crossProduct.GetLength(), 2) == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if three vectors are coplanar.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Return true if three vectors are coplanar.</returns>
        public static bool Coplanarity(XYZ a, XYZ b, XYZ c)
        {
            a = a.RoundVector();
            b = b.RoundVector();
            c = c.RoundVector();
            if (Math.Round(a.TripleProduct(b, c), 2) == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if two vectors are perpendicular.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Returns true if two vectors are perpendicular.</returns>
        public static bool Perpendicular(XYZ a, XYZ b)
        {
            if (Math.Round(a.DotProduct(b), 3) == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get points from the list with maximum distance between them.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="maxDist"></param>
        /// <returns>Returns points pair and maximum distance.</returns>
        public static (XYZ point1, XYZ point2) GetMaxDistancePoints(List<XYZ> points, out double maxDist)
        {
            var client = new FindDistanceClient(points, new StrategyToFindMaxDist());
            maxDist = client.GetDistance();

            return (client.Point1, client.Point2);
        }

        /// <summary>
        /// Get points from the list with minimum distance between them.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="minDist"></param>
        /// <returns>Returns points pair and minimum distance.</returns>
        public static (XYZ point1, XYZ point2) GetMinDistancePoints(List<XYZ> points, out double minDist)
        {
            var client = new FindDistanceClient(points, new StrategyToFindMinDist());
            minDist = client.GetDistance();

            return (client.Point1, client.Point2);
        }

        /// <summary>
        /// Select point from the list which is closest to base point;
        /// </summary>
        /// <param name="basePoint"></param>
        /// <param name="points"></param>
        /// <returns>Return closest point.</returns>
        public static XYZ GetClosestToPoint(XYZ basePoint, List<XYZ> points)
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
        /// Get min and max points whith distance to line is max and min.
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

        /// <summary>
        /// Get vectors from list perpendicular to baseVector.
        /// </summary>
        /// <param name="baseVector"></param>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static List<XYZ> GetPerpendicular(XYZ baseVector, List<XYZ> vectors)
        {
            var perpend = new List<XYZ>();
            foreach (var vector in vectors)
            {
                if (Perpendicular(vector, baseVector))
                {
                    perpend.Add(vector);
                }
            }

            return perpend;
        }

        /// <summary>
        ///  Generate a random XYZ with coordinates as floating-point number that is greater than or equal to 0.0,
        ///  and less than 1.0.
        /// </summary>
        /// <returns></returns>
        public static XYZ GenerateXYZ()
        {
            var rand = new Random();
            rand.NextDouble();
            return new XYZ(rand.NextDouble(), rand.NextDouble(), rand.NextDouble());
        }

        /// <summary>
        /// Get average point between <paramref name="points"/>.
        /// </summary>
        /// <param name="points"></param>
        /// <returns>Retruns average point.</returns>
        public static XYZ GetAverage(List<XYZ> points)
        {
            double x = points.Select(p => p.X).Average();
            double y = points.Select(p => p.Y).Average();
            double z = points.Select(p => p.Z).Average();

            return new XYZ(x, y, z);
        }     

        /// <summary>
        /// Create new Min and Max points from <paramref name="points"/>.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static (XYZ minPoint, XYZ maxPoint) CreateMinMaxPoints(List<XYZ> points)
        {
            //get min max points
            var minx = points.Min(x => x.X);
            var miny = points.Min(x => x.Y);
            var minz = points.Min(x => x.Z);
            var maxx = points.Max(x => x.X);
            var maxy = points.Max(x => x.Y);
            var maxz = points.Max(x => x.Z);
            var min = new XYZ(minx, miny, minz);
            var max = new XYZ(maxx, maxy, maxz);
            return (min, max);
        }

        /// <summary>
        /// Convert <paramref name="initBasisX"/>, <paramref name="initBasisY"/> and <paramref name="initBasisZ"/> to <see cref="Vector3d"/> basis.
        /// </summary>
        /// <param name="initBasisX"></param>
        /// <param name="initBasisY"></param>
        /// <param name="initBasisZ"></param>
        /// <returns>
        /// Returns a new <see cref="Vector3d"/> bases  built by initial coordinates.
        /// </returns>
        public static (Vector3d basisX, Vector3d basisY, Vector3d basisZ) ToBasis3d(XYZ initBasisX, XYZ initBasisY, XYZ initBasisZ)
        {
            return (initBasisX.ToVector3d(), initBasisY.ToVector3d(), initBasisZ.ToVector3d());
        }

        #endregion


        #region PrivateMethods

        /// <summary>
        /// Get matrix's determinant created by 3 vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="vector3"></param>
        /// <returns>Return matrix determinant value</returns>
        private static double GetDeterminantByVectors(XYZ vector1, XYZ vector2, XYZ vector3)
        {
            double[,] matrix = CreateMatrix3D(vector1, vector2, vector3);
            return Matrix.GetMatrixDeterminant(matrix);
        }

        /// <summary>
        /// Create 3D matrix by 3 vectors.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="vector3"></param>
        /// <returns>Return 3D matrix.</returns>
        private static double[,] CreateMatrix3D(XYZ vector1, XYZ vector2, XYZ vector3)
        {
            List<XYZ> result = new List<XYZ>()
            {
                vector1, vector2, vector3
            };

            double[,] matrix = new double[3, 3];

            for (int i = 0; i < 3; i++)
            {
                matrix[i, 0] = result[i].X;
                matrix[i, 1] = result[i].Y;
                matrix[i, 2] = result[i].Z;
            }

            return matrix;
        }


        #endregion
    }
}
