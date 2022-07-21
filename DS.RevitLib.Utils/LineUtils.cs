using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.ModelCurveUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils
{
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
                modelCurveCreator.CreateByPoints(startPoint + offset, endPoint + offset);
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

    }
}
