using Autodesk.Revit.DB;
using System.Collections.Generic;
using DS.MainUtils;
using System;
using System.Linq;
using Ivanov.RevitLib.Utils;
using DS.RevitLib.Utils.MEP;

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
       
    }
}
