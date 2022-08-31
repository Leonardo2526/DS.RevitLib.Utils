using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Lines
{
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
    }
}
