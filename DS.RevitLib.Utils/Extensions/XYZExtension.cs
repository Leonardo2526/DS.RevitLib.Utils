using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Transactions;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Line = Autodesk.Revit.DB.Line;
using Plane = Autodesk.Revit.DB.Plane;

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

        /// <summary>
        /// Specifies if <paramref name="point"/> lies on <paramref name="line"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <param name="canCoinsidence"></param>
        ///  /// <remarks>
        /// Parameter <paramref name="canCoinsidence"/> specifies if <paramref name="point"/> can coinsidence with <paramref name="line"/>'s end points.
        /// </remarks>
        /// <returns>Reruns <see langword="true"/> if <paramref name="point"/> lies inside <paramref name="line"/> segment.
        /// Otherwise returns <see langword="false"/>. 
        /// </returns>
        public static bool OnLine(this XYZ point, Line line, bool canCoinsidence = true)
        {
            var p1 = line.GetEndPoint(0); var p2 = line.GetEndPoint(1);
            return point.IsBetweenPoints(p1, p2, 3, canCoinsidence);
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
        /// <param name="point"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="tolerance">Tolerance in degrees.</param>
        /// <param name="canCoinsidence"></param>
        /// <remarks>
        /// Parameter <paramref name="canCoinsidence"/> specifies if <paramref name="point"/> can coinsidence with <paramref name="point1"/> and <paramref name="point2"/>.
        /// </remarks>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="point"/> is between <paramref name="point1"/> and <paramref name="point2"/>.
        /// <para>
        /// Returns <see langword="false"/> if <paramref name="canCoinsidence"/> is <see langword="false"/> and one of points lies on <paramref name="point"/>.
        /// </para>
        /// </returns>
        public static bool IsBetweenPoints(this XYZ point, XYZ point1, XYZ point2, double tolerance = 3, bool canCoinsidence = true)
        {
            var v1 = (point - point1).Normalize();
            if(!canCoinsidence && v1.IsZeroLength()) { return false; }
            var v2 = (point - point2).Normalize();
            if (!canCoinsidence && v2.IsZeroLength()) { return false; }
            if (v1.IsAlmostEqualTo(v2.Negate(), tolerance.DegToRad()))
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
        public static void Show(this XYZ point, Document doc, double labelSize = 0, ITransactionFactory transactionBuilder = null)
        {
            transactionBuilder ??= new ContextTransactionFactory(doc);
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

            transactionBuilder.CreateAsync(() =>
            {
                var creator = new ModelCurveCreator(doc);
                creator.Create(line1);
                creator.Create(line2);
                creator.Create(line3);
            }, "ShowPoint");
        }

        /// <summary>
        /// Convert <paramref name="point"/> to <see cref="Point3D"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Returns a new <see cref="Point3D"/> built by <paramref name="point"/> coordinates.</returns>
        public static Point3D ToPoint3D(this XYZ point)
        {
            return new Point3D(point.X , point.Y, point.Z);
        }

        /// <summary>
        /// Convert <paramref name="xYZ"/> to <see cref="Vector3d"/>.
        /// </summary>
        /// <param name="xYZ"></param>
        /// <returns>Returns a new <see cref="Vector3d"/> built by <paramref name="xYZ"/> coordinates.</returns>
        public static Vector3d ToVector3d(this XYZ xYZ)
        {
            return new Vector3d(xYZ.X, xYZ.Y, xYZ.Z);
        }

        /// <summary>
        /// Convert <paramref name="xYZ"/> to <see cref="Point3d"/>.
        /// </summary>
        /// <param name="xYZ"></param>
        /// <returns>Returns a new <see cref="Point3d"/> built by <paramref name="xYZ"/> coordinates.</returns>
        public static Point3d ToPoint3d(this XYZ xYZ)
        {
            return new Point3d(xYZ.X, xYZ.Y, xYZ.Z);
        }

    }
}
