using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Points;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Transactions;
using MoreLinq;
using Rhino.Geometry;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Media;
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
            if (!canCoinsidence && v1.IsZeroLength()) { return false; }
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
            if (transactionBuilder is null)
            {
                ShowPoint(point, doc, labelSize);
            }
            else
            {
                transactionBuilder.CreateAsync(() => ShowPoint(point, doc, labelSize), "ShowPoint");
            }
        }

        /// <summary>
        /// Show current point in model as 3 crossing line in this <paramref name="point"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="doc"></param>
        /// <param name="labelSize">Size of label's line to show.</param>
        /// <returns>
        /// Created <see cref="ModelCurve"/>s in <see cref="Document"/>.
        /// </returns>
        public static IEnumerable<ModelCurve> ShowPoint(this XYZ point, Document doc, double labelSize = 0)
        {
            var curves = new List<ModelCurve>();

            labelSize = labelSize == 0 ? 100.MMToFeet() : labelSize;

            Line line1 = Line.CreateBound(
                point + XYZ.BasisX.Multiply(labelSize / 2),
                point - XYZ.BasisX.Multiply(labelSize / 2));

            Line line2 = Line.CreateBound(
               point + XYZ.BasisY.Multiply(labelSize / 2),
               point - XYZ.BasisY.Multiply(labelSize / 2));

            Line line3 = Line.CreateBound(
               point + XYZ.BasisZ.Multiply(labelSize / 2),
               point - XYZ.BasisZ.Multiply(labelSize / 2));

            var creator = new ModelCurveCreator(doc);
            curves.Add(creator.Create(line1));
            curves.Add(creator.Create(line2));
            curves.Add(creator.Create(line3));

            return curves;
        }


        /// <summary>
        /// Convert <paramref name="point"/> to <see cref="Point3D"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Returns a new <see cref="Point3D"/> built by <paramref name="point"/> coordinates.</returns>
        public static Point3D ToPoint3D(this XYZ point)
        {
            return new Point3D(point.X, point.Y, point.Z);
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

        /// <summary>
        /// Convert <paramref name="point"/> to <see cref="Autodesk.Revit.DB.XYZ"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Returns a new <see cref="Autodesk.Revit.DB.XYZ"/> built by <paramref name="point"/> coordinates.</returns>
        public static XYZ ToXYZ(this Point3d point)
        {
            return new XYZ(point.X, point.Y, point.Z);
        }

        /// <summary>
        /// Convert <paramref name="vector"/> to <see cref="Autodesk.Revit.DB.XYZ"/>.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns>Returns a new <see cref="Autodesk.Revit.DB.XYZ"/> built by <paramref name="vector"/> coordinates.</returns>
        public static XYZ ToXYZ(this Vector3d vector)
        {
            return new XYZ(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Transform <paramref name="point"/> with set of <paramref name="transforms"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="transforms"></param>
        /// <returns>
        /// A new transformed <see cref="Autodesk.Revit.DB.XYZ"/>.
        /// </returns>
        public static XYZ Transform(this XYZ point, List<Autodesk.Revit.DB.Transform> transforms)
        {
            XYZ xYZ = new XYZ(point.X, point.Y, point.Z);
            foreach (var transform in transforms)
            {
                xYZ = transform.OfPoint(xYZ);
            }

            return xYZ;
        }

        /// <summary>
        /// Get distance to closest point on floor under the <paramref name="point"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// Distance to closest point on floor under the <paramref name="point"/>.
        /// <para>
        /// If no floors or no floors under the <paramref name="point"/> was found returns <see cref="double.PositiveInfinity"/>.
        /// </para>
        /// </returns>
        public static double GetDistanceToFloor(this XYZ point, Document doc) =>
            GetDistanceToAnyFloor(point, doc, -30);

        /// <summary>
        /// Get distance to closest point on ceiling over the <paramref name="point"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// Distance to closest point on ceiling over the <paramref name="point"/>.
        /// <para>
        /// If no ceiling or no ceiling over the <paramref name="point"/> was found returns <see cref="double.PositiveInfinity"/>.
        /// </para>
        /// </returns>
        public static double GetDistanceToCeiling(this XYZ point, Document doc) =>
            GetDistanceToAnyFloor(point, doc, 30);

        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.XYZ"/> that specifies bound point to floor or ceiling.
        /// </summary>
        /// <remarks>
        /// By default get bound point to floor.
        /// </remarks>
        /// <param name="point"></param>
        /// <param name="doc"></param>
        /// <param name="offsetFromFloor"></param>
        /// <param name="distanceToFindFloor"></param>
        /// <returns>
        /// Bound point to floor if <paramref name="distanceToFindFloor"/> is less than 0 and 
        /// to ceiling if <paramref name="distanceToFindFloor"/> is more than 0.
        /// <para>
        /// <see cref="Autodesk.Revit.DB.XYZ"/> with <see cref="double.MaxValue"/> as Z coordinate if no floor or ceiling was found.
        /// </para>
        /// <para>
        /// <see langword="null"/> if <paramref name="point"/> is outside calculated bound point.
        /// </para>
        /// </returns>
        public static XYZ GetXYZBound(this XYZ point, Document doc, double offsetFromFloor, double distanceToFindFloor = -30)
        {
            XYZ xYZBound = null;

            var zBound = point + XYZ.BasisZ.Multiply(distanceToFindFloor);
            var dir = zBound - point;
            dir = dir.Normalize();

            var hToFloor = distanceToFindFloor > 0 ? point.GetDistanceToCeiling(doc) : point.GetDistanceToFloor(doc);

            //if no floors
            if (hToFloor == double.PositiveInfinity)
            { xYZBound = new XYZ(point.X, point.Y, double.MaxValue); }
            else
            {
                var zOffset = hToFloor - offsetFromFloor;
                if (zOffset >= 0)
                { xYZBound = point + dir.Multiply(zOffset); }
            }

            return xYZBound;
        }

        /// <summary>
        /// Specifies if <paramref name="point1"/> is less than <paramref name="point2"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if each coordinate of <paramref name="point1"/> is less than <paramref name="point2"/>.
        /// <para>
        /// Otherwise returns <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool Less(this XYZ point1, XYZ point2)
           => point1.X < point2.X && point1.Y < point2.Y && point1.Z < point2.Z;

        /// <summary>
        /// Specifies if <paramref name="point1"/> is more than <paramref name="point2"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if each coordinate of <paramref name="point1"/> is more than <paramref name="point2"/>.
        /// <para>
        /// Otherwise returns <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool More(this XYZ point1, XYZ point2)
           => point1.X > point2.X && point1.Y > point2.Y && point1.Z > point2.Z;

        /// <summary>
        /// Get normal to <paramref name="dir"/> from 
        /// <see cref="Autodesk.Revit.DB.XYZ.BasisX"/>, <see cref="Autodesk.Revit.DB.XYZ.BasisY"/> or <see cref="Autodesk.Revit.DB.XYZ.BasisZ"/>.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns>
        /// Normal vector from XYZ base orths.
        /// <para>
        /// Random normal if no normal vectors to <paramref name="dir"/> from XYZ base orths exists.
        /// </para>
        /// </returns>
        public static XYZ GetPerpendicular(this XYZ dir)
        {
            var dir3d = dir.ToVector3d().Round(3);
            if (dir3d.IsPerpendicularTo(XYZ.BasisX.ToVector3d())) { return XYZ.BasisX; }
            else if (dir3d.IsPerpendicularTo(XYZ.BasisY.ToVector3d())) { return XYZ.BasisY; }
            else if (dir3d.IsPerpendicularTo(XYZ.BasisZ.ToVector3d())) { return XYZ.BasisZ; }
            else
            { return dir.GetRandomPerpendicular(XYZ.Zero); }
        }

        /// <summary>
        /// Get collisions between <paramref name="point"/> and <paramref name="inputElements"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="inputElements"></param>
        /// <param name="tolerance"></param>
        /// <returns>
        /// Collisions between <paramref name="point"/> and <paramref name="inputElements"/>
        /// by <see cref="Autodesk.Revit.DB.BoundingBoxContainsPointFilter"/>.
        /// <para>
        /// Empty list if no collisions was found.
        /// </para>
        /// </returns>
        public static List<(XYZ, Element)> GetBoxXYZCollisions(this XYZ point, List<Element> inputElements, double tolerance = 0.01)
        {
            var collisions = new List<(XYZ, Element)>();

            Document doc = inputElements.FirstOrDefault().Document;
            var collector = new FilteredElementCollector(doc, inputElements.Select(el => el.Id).ToList());
            var filter = new BoundingBoxContainsPointFilter(point, tolerance);
            var collisionElements = collector.WherePasses(filter).ToElements();

            collisionElements.ForEach(e => collisions.Add((point, e)));

            return collisions;
        }

        /// <summary>
        /// Get collisions between <paramref name="point"/> and <paramref name="inputElements"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="inputElements"></param>
        /// <param name="tolerance"></param>
        /// <returns>
        ///  Collisions between <paramref name="point"/> and <paramref name="inputElements"/>.
        /// <para>
        /// Empty list if no collisions was found.
        /// </para>
        /// </returns>
        public static List<(XYZ, Element)> GetCollisions(this XYZ point, List<Element> inputElements, double tolerance = 0.01)
        {
            var collisions = new List<(XYZ, Element)>();

            var boxXYZcollisions = point.GetBoxXYZCollisions(inputElements, tolerance);
            if (!boxXYZcollisions.Any()) { return collisions; }

            foreach (var collision in boxXYZcollisions)
            {
                var solid = ElementUtils.GetSolid(collision.Item2);
                if (solid.Contains(point, true)) { collisions.Add(collision); }
            }

            return collisions;
        }

        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.Outline"/> from <paramref name="point"/> with <paramref name="offset"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="offset"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Outline"/> build around <paramref name="point"/>.
        /// </returns>
        public static Outline GetOutline(this XYZ point, double offset)
        {
            var moveVector = new XYZ(offset, offset, offset);
            var p1 = point + moveVector;
            var p2 = point - moveVector;

            var (minPoint, maxPoint) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p1, p2 });
            return new Outline(minPoint, maxPoint);
        }

        #region Private methods

        /// <summary>
        /// Get distance to closest point on floor under or over the <paramref name="point"/>.     
        /// </summary>
        /// <remarks>
        /// <paramref name="findDist"/> specifies +/- direction and it's value to find floors.
        /// </remarks>
        /// <param name="point"></param>
        /// <param name="doc"></param>
        /// <param name="findDist"></param>
        /// <returns>
        /// Distance to closest point on floor under or over the <paramref name="point"/>.
        /// <para>
        /// If no floors or no floors under or over the <paramref name="point"/> was found returns <see cref="double.PositiveInfinity"/>.
        /// </para>
        /// </returns>
        private static double GetDistanceToAnyFloor(this XYZ point, Document doc, double findDist)
        {
            var offset = 0.5;
            var p1 = point + new XYZ(-offset, -offset, 0);
            var p2 = point + new XYZ(offset, offset, findDist);

            var (minPoint, maxPoint) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p1, p2 });
            var outline = new Outline(minPoint, maxPoint);
            var floors = findDist > 0 ? doc.GetCeilings(outline) : doc.GetFloors(outline);
            if (floors.Count == 0) { return double.PositiveInfinity; }

            var line = Line.CreateBound(new XYZ(point.X, point.Y, point.Z), new XYZ(point.X, point.Y, point.Z + findDist));
            var opt = new SolidCurveIntersectionOptions() { ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside };

            var intersections = new List<SolidCurveIntersection>();
            foreach (var f in floors)
            {
                var solid = f.GetSolidInLink(doc);
                var intersectResult = solid.IntersectWithCurve(line, opt);
                if (intersectResult.SegmentCount > 0)
                { intersections.Add(intersectResult); }
            }
            if (intersections.Count == 0) { return double.PositiveInfinity; ; }
            intersections.OrderBy(x => x.GetCurveSegment(0).Length);

            var result = intersections.Min(x => x.GetCurveSegment(0).Length);

            return Math.Abs(result - line.Length) < 0.001 ? double.PositiveInfinity : result;
        }

        #endregion

    }
}
