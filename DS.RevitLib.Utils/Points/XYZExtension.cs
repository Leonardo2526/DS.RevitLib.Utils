using Autodesk.Revit.DB;
using System.Collections.Generic;
using System;
using System.Linq;
using Ivanov.RevitLib.Utils;
using DS.RevitLib.Utils.MEP;
using DS.ClassLib.VarUtils;

namespace DS.RevitLib.Utils.Extensions
{

    public static class XYZExtension

    {
        /// <summary>
        /// Get XYZ coordinate which value is not zero.
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns>Returns one of not zero coordinate. Throw exception if multiple not zero values exist.</returns>
        public static int GetNotZeroCoordinate(this XYZ xyz)
        {
            var coordinates = new List<int>();

            int prc = 0;
            int coordNorm;

            coordNorm = xyz.X.Normalize();
            if (coordNorm.CompareTo(prc) > 0)
            {
                coordinates.Add((int)Math.Round(xyz.X));
            }
            coordNorm = xyz.Y.Normalize();
            if (coordNorm.CompareTo(prc) > 0)
            {
                coordinates.Add((int)Math.Round(xyz.Y));
            }
            coordNorm = xyz.Z.Normalize();
            if (coordNorm.CompareTo(prc) > 0)
            {
                coordinates.Add((int)Math.Round(xyz.Z));
            }

            if (coordinates.Count > 1)
            {
                throw new InvalidOperationException("XYZ has multiple not zero values.");
            }

            return coordinates.FirstOrDefault();
        }

        /// <summary>
        /// Get distance between two point by reference base point.
        /// </summary>
        /// <param name="basePoint"></param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns>Returns positive value if startPoint is between base point and endPoint. 
        /// Returns negative value if endPoint is between base point and startPoint</returns>
        public static double DistanceToByBase(this XYZ startPoint, XYZ basePoint, XYZ endPoint)
        {
            XYZ startVector = startPoint - basePoint;
            XYZ endVector = endPoint - basePoint;
            XYZ difVector = endVector - startVector;

            double angle = endVector.AngleTo(difVector);
            double angleDeg = angle.RadToDeg();

            int prc = 15;
            if (angleDeg.CompareTo(prc) < 0)
            {
                return difVector.GetLength();
            }
            else
            {
                return -difVector.GetLength();
            }
        }

        /// <summary>
        /// Check if point is on plane.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Return true if it's. Return false if it isn't.</returns>
        public static bool IsPointOntoPlane(this XYZ point, Plane plane)
        {
            XYZ proj = plane.ProjectOnto(point);
            XYZ vector = point - proj;

            if (vector.RoundVector(1).IsZeroLength())
            {
                return true;
            }

            return false;
        }

        public static XYZ RoundVector(this XYZ vector, int value = 3)
        {
            double x = Math.Round(vector.X, value);
            double y = Math.Round(vector.Y, value);
            double z = Math.Round(vector.Z, value);

            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Check if current point lies inside segment between point1 and point2.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static bool IsBetweenPoints(this XYZ point, XYZ point1, XYZ point2)
        {
            var v1 = (point - point1).RoundVector().Normalize();
            var v2 = (point2 - point).RoundVector().Normalize();
            var v21 = (point2 - point1).RoundVector().Normalize();

            if (v1.IsZeroLength() || v2.IsZeroLength() || v21.IsZeroLength())
            {
                throw new ArgumentException("Points overlap");
            }


            if (!XYZUtils.Collinearity(v1, v21))
            {
                return false;
            }

            if (v1.X != 0)
            {
                if (point1.X < point.X && point2.X > point.X)
                {
                    return true;
                }
                else if (point2.X < point.X && point1.X > point.X)
                {
                    return true;
                }
            }
            else if (v1.Y != 0)
            {
                if (point1.Y < point.Y && point2.Y > point.Y)
                {
                    return true;
                }
                else if (point2.Y < point.Y && point1.Y > point.Y)
                {
                    return true;
                }
            }
            else if (v1.Z != 0)
            {
                if (point1.Z < point.Z && point2.Z > point.Z)
                {
                    return true;
                }
                else if (point2.Z < point.Z && point1.Z > point.Z)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get a random normilise perpendicular vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="basePoint">Common point of two perpendicular vectors.</param>
        /// <returns></returns>
        public static XYZ GetRandomPerpendicular(this XYZ vector, XYZ basePoint = null)
        {
            basePoint ??= new XYZ(0,0,0);

            XYZ randPoint = XYZUtils.GenerateXYZ();
            XYZ randVector = randPoint - basePoint;
            return vector.CrossProduct(randVector).RoundVector().Normalize();
        }

    }
}
