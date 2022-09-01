﻿using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Points.XYZAlgorithms.MaxDistance;
using DS.RevitLib.Utils.Points.XYZAlgorithms.MaxDistance.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils
{
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
            if (Math.Round(a.DotProduct(b) , 3) ==0)
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
            var client = new MaxDistanceClient(points, new NaiveStrategy());
            maxDist = client.GetMaxDistance();

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
