using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Lines
{
    /// <summary>
    /// Extension methods for 'Line' objects.
    /// </summary>
    public static class LineExtensions
    {
        /// <summary>
        /// Возвращает новую линию, увеличенную в каждую сторону на заданное расстояние
        /// </summary>
        /// <param name="line">Исходная линия</param>
        /// <param name="len">Длина, на которую нужно увеличить линию в каждую сторону</param>
        public static Line IncreaseLength(this Line line, double len)
        {
            var p1 = line.GetEndPoint(0);
            var p2 = line.GetEndPoint(1);

            var v = (p2 - p1).Normalize();

            p1 += v.Negate().Multiply(len);
            p2 += v.Multiply(len);

            return Line.CreateBound(p1, p2);
        }

        /// <summary>
        /// Возвращает новую линию, уменьшенную в каждую сторону на заданное расстояние
        /// </summary>
        /// <param name="line">Исходная линия</param>
        /// <param name="len">Длина, на которую нужно увеличить линию в каждую сторону</param>
        public static Line ReduceLength(this Line line, double len)
        {
            var p1 = line.GetEndPoint(0);
            var p2 = line.GetEndPoint(1);

            var v = (p2 - p1).Normalize();

            p1 -= v.Negate().Multiply(len);
            p2 -= v.Multiply(len);

            return Line.CreateBound(p1, p2);
        }

        /// <summary>
        /// Get center of line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>Returns center point of given line.</returns>
        public static XYZ GetCenter(this Line line)
        {
            XYZ startPoint = line.GetEndPoint(0);
            XYZ endPoint = line.GetEndPoint(1);
            return new XYZ((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2, (startPoint.Z + endPoint.Z) / 2);
        }

        /// <summary>
        /// Cut line between specified points.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="cuttedLine"></param>
        /// <returns>Returns two splitted lines.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static (Line line1, Line line2) Cut(this Line line, XYZ p1, XYZ p2, out Line cuttedLine)
        {
            XYZ lp1 = line.GetEndPoint(0);
            XYZ lp2 = line.GetEndPoint(1);

            if (!p1.IsBetweenPoints(lp1, lp2))
            {
                throw new ArgumentException($"Point {p1} is outside of line boundaries.");
            }
            if (!p2.IsBetweenPoints(lp1, lp2))
            {
                throw new ArgumentException($"Point {p2} is outside of line boundaries.");
            }

            List<XYZ> points = new List<XYZ> { lp1, lp2, p1, p2 };
            points = points.OrderBy(obj => obj.DistanceTo(lp1)).ToList();


            Line line1 = Line.CreateBound(points[0], points[1]);
            cuttedLine = Line.CreateBound(points[1], points[2]);
            Line line2 = Line.CreateBound(points[2], points[3]);

            return (line1, line2);
        }

        /// <summary>
        /// Create ModelCurve by given line.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="doc"></param>
        public static void Show(this Line line, Document doc)
        {
            var creator = new ModelCurveCreator(doc);
            creator.Create(line.GetEndPoint(0), line.GetEndPoint(1));
        }

        /// <summary>
        /// Get normal <see cref="Autodesk.Revit.DB.XYZ"/> vector to <paramref name="line"/> at <paramref name="point"/>.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <returns>Returns normal from <see cref="Autodesk.Revit.DB.Plane"/> built by <paramref name="line"/> and <paramref name="point"/>.
        /// <para>Returns normal from random <see cref="Autodesk.Revit.DB.Plane"/> built by <paramref name="line"/> if point is <see langword="null."></see></para>
        /// </returns>
        public static XYZ GetNormal(this Line line, XYZ point = null)
        {
            return line.GetPlane(point).Normal;
        }

        /// <summary>
        /// Get <see cref="Basis"/> from <paramref name="line"/>.
        /// <para>
        /// If <paramref name="basisY"/> is null random <see cref="Autodesk.Revit.DB.XYZ"/> perpendicular to <paramref name="line"/> direction will be set.
        /// If <paramref name="basePoint"/> is null <paramref name="line"/>'s origin will be set.
        /// </para>
        /// </summary>
        /// <param name="line"></param>
        /// <param name="basePoint"></param>
        /// <param name="basisY"></param>
        /// <returns>Returns <see cref="Basis"/> from <paramref name="line"/>. 
        /// </returns>
        public static Basis GetBasis(this Line line, XYZ basePoint = null, XYZ basisY = null)
        {
            XYZ basisX = line.Direction;            
            basisY ??= basisX.GetRandomPerpendicular();
            XYZ basisZ = basisX.CrossProduct(basisY);
            basePoint ??= line.Origin;
            return new Basis(basisX, basisY, basisZ, basePoint);
        }


        /// <summary>
        /// Specifies if <paramref name="point"/> lies on <paramref name="line"/>.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="point"/> is between <paramref name="line"/>'s end points or coincidense with them.
        /// <para>
        /// Otherwise returns <see langword="false"/>.     
        /// </para>
        /// </returns>
        public static bool Contains(this Line line, XYZ point, double tolerance = 1.00e-3)
        {
            Rhino.Geometry.Line rline = new(line.GetEndPoint(0).ToPoint3d(), line.GetEndPoint(1).ToPoint3d());
            var rpoint = point.ToPoint3d();
            var d = rline.DistanceTo(rpoint, true);
            return d < tolerance;
        }
    }
}
