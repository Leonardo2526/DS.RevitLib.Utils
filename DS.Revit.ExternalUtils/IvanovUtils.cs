using Autodesk.Revit.DB;
using iUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.Revit.ExternalUtils
{
    public static class IvanovUtils
    {

        /// <summary>
        /// Проверяет пересекаются ли 2 <see cref="BoundingBoxXYZ"/>
        /// </summary>
        /// <param name="b1">Первый bb</param>
        /// <param name="b2">Второй bb</param>
        /// <returns>Возвращает true если боксы пересекаются, иначе - false</returns>
        public static bool IsBBIntersect(this BoundingBoxXYZ b1, BoundingBoxXYZ b2)
        {
            var transfomr = b1.Transform;
            var xmin1 = transfomr.OfPoint(b1.Min).X;
            var xmax1 = transfomr.OfPoint(b1.Max).X;
            var ymin1 = transfomr.OfPoint(b1.Min).Y;
            var ymax1 = transfomr.OfPoint(b1.Max).Y;
            var zmin1 = transfomr.OfPoint(b1.Min).Z;
            var zmax1 = transfomr.OfPoint(b1.Max).Z;

            var xmin2 = b2.Min.X;
            var xmax2 = b2.Max.X;
            var ymin2 = b2.Min.Y;
            var ymax2 = b2.Max.Y;
            var zmin2 = b2.Min.Z;
            var zmax2 = b2.Max.Z;

            Func<double, double, double, double, bool> overlap1D = (min1, max1, min2, max2) => { return max1 >= min2 && max2 >= min1; };

            return overlap1D(xmin1, xmax1, xmin2, xmax2) && overlap1D(ymin1, ymax1, ymin2, ymax2) && overlap1D(zmin1, zmax1, zmin2, zmax2);
        }


        /// <summary>
        /// Получает элемент на другой стороне заданного коннектора
        /// </summary>
        /// <param name="c">Коннектор</param>
        public static Connector GetReferenceElementConnector(this Connector c)
         => c.AllRefs.Cast<Connector>().Where(x => x.Owner.Id != c.Owner.Id && x.Owner is not InsulationLiningBase).FirstOrDefault();


        /// <summary>
        /// Вычисляет ближайший коннектор для данных элементов и заданной точки
        /// </summary>
        /// <param name="elems">Список элементов для поиска</param>
        /// <param name="point">Точка дя поиска ближайшего коннектора</param>
        public static Connector FindNearestConnector(List<MEPCurve> elems, XYZ point)
         => elems.SelectMany(x => x.ConnectorManager.Connectors.Cast<Connector>().ToList())
                .OrderBy(x => (x.Origin - point).AbsXYZ().GetLength()).FirstOrDefault();

        public static XYZ AbsXYZ(this XYZ p) => new XYZ(Math.Abs(p.X), Math.Abs(p.Y), Math.Abs(p.Z));

        /// <summary>
        /// Возвращает два коннектора для соединения 2 элементов
        /// </summary>
        /// <param name="e1">Элемент 1</param>
        /// <param name="e2">Элемент 2</param>
        /// <returns></returns>
        public static (Connector c1, Connector c2) GetConnectors(MEPCurve e1, MEPCurve e2)
        {
            var c1 = e1.GetCurve();
            var c2 = e2.GetCurve();
            c1.MakeUnbound();
            c2.MakeUnbound();
            c1.Intersect(c2, out var resultArr);
            var point = resultArr.get_Item(0).XYZPoint;

            var con1 = e1.GetConnector(point);
            var con2 = e2.GetConnector(point);
            return (con1, con2);
        }

        /// <summary>
        /// Возвращает коннектор заданного элемента, в заданной точке
        /// </summary>
        /// <param name="point">Точка для поиска коннектора</param>
        /// <param name="element">Элемент для поиска коннектора</param>
        public static Connector GetConnector(this MEPCurve element, XYZ point)
         => element.ConnectorManager.Connectors.Cast<Connector>().FirstOrDefault(x => x.Origin.IsAlmostEqualTo(point, 1e-4));
      


        /// <summary>
        /// Проверяет находитсся ли элемент рядом с линиями
        /// </summary>
        /// <param name="element">Элемент для проверки</param>
        /// <param name="allPoints">Все точки линий</param>
        /// <returns></returns>
        private static bool IsElementNearLines(Element element, List<XYZ> allPoints)
        {
            var bb = element.get_BoundingBox(null);
            if (bb == null) return true;

            var diag = (bb.Max - bb.Min).GetLength() / 1.8;

            foreach (var p in allPoints)
            {
                if ((p - bb.Max).GetLength() < diag)
                    return true;
                else if ((p - bb.Min).GetLength() < diag)
                    return true;
            }


            return false;
        }


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
        /// Возвращает высоту, ширину и площадь элемента
        /// </summary>
        /// <param name="curve">Труба, воздуховод или лоток</param>
        public static (double height, double width, double area) GetMEPCurveDimensions(MEPCurve curve)
        {
            var doc = curve.Document;
            var type = doc.GetElement(curve.GetTypeId()) as MEPCurveType;
            var shape = type.Shape;
            switch (shape)
            {
                case ConnectorProfileType.Invalid:
                    return (1, 1, 1);
                case ConnectorProfileType.Round:
                    return (curve.Diameter, curve.Diameter, Math.PI * curve.Diameter / 4);
                case ConnectorProfileType.Rectangular:
                    return (curve.Height, curve.Width, curve.Height * curve.Width);
                case ConnectorProfileType.Oval:
                    return (curve.Height, curve.Width, curve.Height * curve.Width);
            }

            return (1, 1, 1);
        }


        /// <summary>
        /// Получает все точки из солида
        /// </summary>
        /// <param name="s">Солид</param>
        public static List<XYZ> GetAllPointsFromSolid(Solid s)
        {
            var curves = s.Edges.Cast<Edge>().Select(x => x.AsCurve()).ToList();
            var result = new List<XYZ>();

            curves.ForEach(x => result.Add(x.GetEndPoint(0)));
            curves.ForEach(x => result.Add(x.GetEndPoint(1)));

            return result;
        }


        /// <summary>
        /// Вычисляет стартовую и финишную точки на элементе, который изменяться
        /// </summary>
        /// <param name="bandableCurve">Элемент, который будет изменяться</param>
        /// <param name="resultSolid">Результирующий солид пересечения коллизии</param>
        /// <returns>Возвращает первую и последнюю точки, спроецированные на изменяемый элемент</returns>
        public static (XYZ startPoint, XYZ endPoint) GetStartEndPointnsToCutMEPCurve(MEPCurve bandableCurve, Solid resultSolid)
        {
            var points = GetAllPointsFromSolid(resultSolid);
            var projects = points.Select(x => bandableCurve.GetCurve().Project(x)).OrderBy(x => x.Parameter).ToList();
            if (projects == null && !projects.Any())
                throw new NullReferenceException($"Не удалось спроецировать результрующий солит на объект {bandableCurve.Id}:{bandableCurve.Name}");
            return (projects.First().XYZPoint, projects.Last().XYZPoint);
        }
       
        private static bool IsPointInElementBB(XYZ point, Element e)
        {
            var bb = e.get_BoundingBox(null);
            if (bb == null) return false;
            return IsPointInElementBB(point, bb);
        }

        private static bool IsPointInElementBB(XYZ point, BoundingBoxXYZ bb)
        {
            if (point.X >= bb.Min.X && point.Y >= bb.Min.Y && point.Z >= bb.Min.Z
                      && point.X < bb.Max.X && point.Y < bb.Max.Y && point.Z < bb.Max.Z)
                return true;
            return false;
        }
    }
}
