using Autodesk.Revit.DB;
using System.Collections.Generic;
using DS.MainUtils;
using System;
using System.Linq;
using Ivanov.RevitLib.Utils;

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
            double distBase_Start = basePoint.DistanceTo(startPoint);
            double distBase_End = basePoint.DistanceTo(endPoint);

            XYZ vectorBase1 = (basePoint - startPoint).AbsXYZ();
            XYZ vectorBase2 = (basePoint - endPoint).AbsXYZ();

            double angleRad = vectorBase1.AngleTo(vectorBase2);
            double angleDeg = angleRad * 180 / Math.PI;

            int normCoordStart = GetNormCoordinate(startPoint, basePoint, angleRad);
            int normCoordEnd = GetNormCoordinate(endPoint, basePoint, angleRad);

            double distance = (distBase_End * normCoordEnd - distBase_Start * normCoordStart) * normCoordEnd;

            return distance;
        }

        private static int GetNormCoordinate(XYZ p1, XYZ p2, double angleRad)
        {
            XYZ vector = p1 - p2;

            vector = new XYZ(vector.X * Math.Sin(angleRad), vector.X * Math.Cos(angleRad), vector.Z);

            XYZ vectorNorm = vector.Normalize();

            return vectorNorm.GetNotZeroCoordinate();
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

            if (vector.RoundVector(0.001).IsZeroLength())
            {
                return true;
            }

            return false;
        }

        public static XYZ RoundVector(this XYZ vector, double value = 1e-12)
        {
            double x = vector.X;
            double y = vector.Y;
            double z = vector.Z;

            if (Math.Abs(x) < value)
                x = 0;
            if (Math.Abs(y) < value)
                y = 0;
            if (Math.Abs(z) < value)
                z = 0;
            return new XYZ(x, y, z);
        }
    }
}
