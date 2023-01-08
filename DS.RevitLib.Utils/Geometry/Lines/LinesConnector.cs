using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Lines
{
    /// <summary>
    /// Tool to connect lines.
    /// </summary>
    public class LinesConnector
    {
        private readonly List<Line> _lines;

        /// <summary>
        /// Create a new instance of object to connect neighbout lines. 
        /// </summary>
        /// <param name="lines"></param>
        public LinesConnector(List<Line> lines)
        {
            _lines = lines;
        }

        /// <summary>
        /// Connect lines.
        /// </summary>
        /// <returns>Returns new lines built by intersection point of neighbor lines from input.</returns>
        public List<Line> Connect()
        {
            var points = GetIntersectionPoints();
            return LineUtils.GetLines(points);
        }

        private List<XYZ> GetIntersectionPoints()
        {
            var points = new List<XYZ>();

            XYZ startPoint = LineUtils.GetIntersectionPoint(_lines.First(), _lines.Last());
            points.Add(startPoint);
            for (int i = 0; i < _lines.Count - 1; i++)
            {
                XYZ point = LineUtils.GetIntersectionPoint(_lines[i], _lines[i + 1]);
                points.Add(point);
            }
            points.Add(startPoint);

            return points.Where(obj => obj is not null).ToList();
        }
    }
}
