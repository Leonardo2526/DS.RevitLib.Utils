using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Object representing extension methods to work with XYZ points.
    /// </summary>
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
            var v1 = (point - point1).Normalize();
            var v2 = (point - point2).Normalize();
            if (v1.IsAlmostEqualTo(v2.Negate(), 3.DegToRad()))
            {
                return true;
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
            basePoint ??= new XYZ(0, 0, 0);

            XYZ randPoint = XYZUtils.GenerateXYZ();
            XYZ randVector = randPoint - basePoint;
            return vector.CrossProduct(randVector).RoundVector().Normalize();
        }

        /// <summary>
        /// Get line from the list with minimum distnace to given poin.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static Line GetClosestLine(this XYZ point, List<Line> lines)
        {
            double resDist = lines.First().Distance(point);
            Line resLine = lines.First();

            foreach (var line in lines)
            {
                double dist = line.Distance(point);
                if (dist < resDist)
                {
                    resDist = dist;
                    resLine = line;
                }
            }

            return resLine;
        }

        /// <summary>
        /// Show current point in model as 3 crossing line in this <paramref name="point"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="transactionBuilder"></param>
        /// <param name="doc"></param>
        /// <param name="labelSize">Size of label's line to show.</param>
        public static void Show(this XYZ point, Document doc, double labelSize = 0, AbstractTransactionBuilder transactionBuilder = null)
        {
            transactionBuilder ??= new TransactionBuilder_v1<Element>(doc);
            labelSize = labelSize == 0 ? 100.mmToFyt2() : labelSize;

            Line line1 = Line.CreateBound(
                point + XYZ.BasisX.Multiply(labelSize / 2),
                point - XYZ.BasisX.Multiply(labelSize / 2));

            Line line2 = Line.CreateBound(
               point + XYZ.BasisY.Multiply(labelSize / 2),
               point - XYZ.BasisY.Multiply(labelSize / 2));

            Line line3 = Line.CreateBound(
               point + XYZ.BasisZ.Multiply(labelSize / 2),
               point - XYZ.BasisZ.Multiply(labelSize / 2));

            transactionBuilder.Build(() =>
            {
                var creator = new ModelCurveCreator(doc);
                creator.Create(line1);
                creator.Create(line2);
                creator.Create(line3);
            }, "ShowPoint");
        }

    }
}
