using Autodesk.Revit.DB;
using DS.RevitLib.Utils.ModelCurveUtils;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils
{
    /// <summary>
    /// Tools to work with lines.
    /// </summary>
    public static class LineUtils
    {
        public static Line CreateCenterLine(Element element, XYZ offset = null, bool show = false)
        {
            ElementUtils.GetPoints(element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPointElement);

            if (offset == null)
                offset = new XYZ();

            if (show)
            {
                ModelCurveCreator modelCurveCreator = new ModelCurveCreator(element.Document);
                modelCurveCreator.Create(startPoint + offset, endPoint + offset);
            }

            return Line.CreateBound(startPoint + offset, endPoint + offset);
        }

        public static XYZ GetClosestToLine(Line line, XYZ point1, XYZ point2)
        {
            double length1 = line.Distance(point1);
            double length2 = line.Distance(point2);
            if (length1 > length2)
            {
                return point2;
            }

            return point1;
        }

        /// <summary>
        /// Get point with minimum distance to line.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public static XYZ GetClosestToLine(Line line, List<XYZ> points)
        {
            double resDist = 10000;
            XYZ resPoint = null;
            foreach (var point in points)
            {
                double currentDist = line.Distance(point);
                if (currentDist < resDist)
                {
                    resDist = currentDist;
                    resPoint = point;
                }
            }

            return resPoint;
        }

        /// <summary>
        /// Select line from the list which has minimum length;
        /// </summary>
        /// <param name="lines"></param>
        /// <returns>Return shortest line.</returns>
        public static Line SelectShortest(List<Line> lines)
        {
            Line line = lines.FirstOrDefault();
            double distance = line.Length;

            if (lines.Count > 1)
            {
                for (int i = 1; i < lines.Count; i++)
                {
                    double curDistance = lines[i].Length;
                    if (curDistance < distance)
                    {
                        distance = curDistance;
                        line = lines[i];
                    }
                }
            }

            return line;
        }      

        /// <summary>
        /// Create lines list from <paramref name="points"/>.
        /// </summary>
        /// <param name="points"></param>
        /// <returns>Returns list of lines created by given points.</returns>
        public static List<Line> GetLines(List<XYZ> points)
        {
            var lines = new List<Line>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                var line = Line.CreateBound(points[i], points[i + 1]);
                lines.Add(line);
            }
            return lines;
        }

    }
}
