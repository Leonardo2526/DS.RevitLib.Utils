using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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

            if (!p1.IsBetweenPoints(lp1, lp2) | !p2.IsBetweenPoints(lp1, lp2))
            {
                throw new ArgumentException("input points are outside line boundaries.");
            }

            List<XYZ> points = new List<XYZ> { lp1, lp2, p1, p2 };
            points = points.OrderBy(obj => obj.DistanceTo(lp1)).ToList();


            Line line1 = Line.CreateBound(points[0], points[1]);
            cuttedLine = Line.CreateBound(points[1], points[2]);
            Line line2 = Line.CreateBound(points[2], points[3]);

            return (line1, line2);
        }
    }
}
