using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry.Lines;
using DS.RevitLib.Utils.ModelCurveUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        /// <summary>
        /// Get intersection point of two lines.
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="enableVirtual1"></param>
        /// <param name="enableVirtual2"></param>
        /// <remarks>
        /// Parameter <paramref name="enableVirtual1"/> specifies if virtual intersection is 
        /// enable between <paramref name="line1"/> and <paramref name="line2"/>.
        /// <para>
        /// Parameter <paramref name="enableVirtual2"/> specifies if virtual intersection is 
        /// enable between <paramref name="line2"/> and <paramref name="line1"/>.      
        /// </para>
        /// </remarks>
        /// <returns>Returns intersection point if unbound lines of
        /// <paramref name="line1"/> and <paramref name="line2"/> have intersection.
        /// <para>
        /// Otherwise returns <see langword="null"/>.    
        /// </para>
        /// </returns>
        public static XYZ GetIntersectionPoint(Line line1, Line line2, bool enableVirtual1 = true, bool enableVirtual2 = true)
        {
            Line uLine1 = enableVirtual1 ?
                Line.CreateUnbound(line1.Origin, line1.Direction) : line1;
            Line uLine2 = enableVirtual2 ?
                Line.CreateUnbound(line2.Origin, line2.Direction) : line2;

            var intersect = uLine1.Intersect(uLine2, out IntersectionResultArray resultArray);
       
            return resultArray is not null && resultArray.Size > 0 ?
                resultArray.get_Item(0).XYZPoint :
                null;
        }

        /// <summary>
        /// Get <see cref="LineOverlapResult"/> between two lines intersection.
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="enableVirtual1"></param>
        /// <param name="enableVirtual2"></param>
        /// <remarks>
        /// Parameter <paramref name="enableVirtual1"/> specifies if virtual intersection is 
        /// enable between <paramref name="line1"/> and <paramref name="line2"/>.
        /// <para>
        /// Parameter <paramref name="enableVirtual2"/> specifies if virtual intersection is 
        /// enable between <paramref name="line2"/> and <paramref name="line1"/>.      
        /// </para>
        /// </remarks>
        /// <returns>Returns <see cref="LineOverlapResult"/> if lines have <see cref="SetComparisonResult.Overlap"/> intersection.
        /// <para>
        /// Otherwise returns <see cref="LineOverlapResult.None"/>.    
        /// </para>
        /// </returns>
        public static LineOverlapResult GetOverlapResult(Line line1, Line line2, bool enableVirtual1 = true, bool enableVirtual2 = true)
        {
            Line uLine1 = enableVirtual1 ?
             Line.CreateUnbound(line1.Origin, line1.Direction) : line1;
            Line uLine2 = enableVirtual2 ?
                Line.CreateUnbound(line2.Origin, line2.Direction) : line2;

            //check unbound intersection
            var uIntersect = uLine1.Intersect(uLine2, out IntersectionResultArray resultArray);
            if (uIntersect != SetComparisonResult.Overlap) { return LineOverlapResult.None; }

            var point = resultArray.get_Item(0).XYZPoint;
            if (point.OnLine(line1, false) && point.OnLine(line2, false))
            { return LineOverlapResult.SegementOverlap; }
            else if (point.OnLine(line1, false) | point.OnLine(line2, false))
            { return LineOverlapResult.SegmentPointOverlap; }
            else
            { return LineOverlapResult.PointOverlap; }
        }

            /// <summary>
            /// Specifies if <paramref name="line1"/> and <paramref name="line2"/> lie on the same plane (coplanar lines).
            /// </summary>
            /// <param name="line1"></param>
            /// <param name="line2"></param>
            /// <returns>Returns <see langword="true"/> if <paramref name="line1"/> and <paramref name="line2"/> are coplanar. 
            /// Otherwise returns <see langword="false"/>.        
            /// </returns>
            public static bool Coplanarity(Line line1, Line line2)
        {
            var v1 = line1.Direction; var v2 = line2.Direction;
            var v3 = (line1.Origin - line2.Origin).Normalize();
            return Math.Round(v1.TripleProduct(v2, v3), 5) == 0;
        }

        /// <summary>
        /// Get parent/child relation between <paramref name="line1"/> and <paramref name="line2"/>.
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="inverted"></param>       
        /// <returns>Returns <paramref name="line1"/> as parent and <paramref name="line2"/> as child 
        /// if lines intersection point (real or virtual) lies on <paramref name="line1"/>.
        /// <para>
        /// Parameter <paramref name="inverted"/> returns <see langword="false"/> in this case.    
        /// </para>     
        /// Returns <paramref name="line2"/> as parent and <paramref name="line1"/> as child
        /// if lines intersection point (real or virtual) lies on <paramref name="line2"/>.
        /// <para>
        /// Parameter <paramref name="inverted"/> returns <see langword="true"/> in this case.
        /// </para>
        ///  <para>
        /// Returns (<see langword="null"/>, <see langword="null"/>) if no intersections was found or unable to detect relation.  
        /// </para>
        /// </returns>
        public static (Line parentLine, Line childLine) GetRelation(Line line1, Line line2, out bool inverted)
        {
            inverted = false;
            var pointsList = new List<XYZ>()
            {
                line1.GetEndPoint(0),
                line1.GetEndPoint(1),
                line2.GetEndPoint(0),
                line2.GetEndPoint(1)
            };

            //check if lines end points coincidense.
            XYZUtils.GetMinDistancePoints(pointsList, out double minDist);
            if(Math.Round(minDist, 3) == 0) { return (null, null); }

            var point = GetIntersectionPoint(line1, line2, false, true);
            if (point != null && !point.OnLine(line2)) { return (line1, line2); }

            point = GetIntersectionPoint(line1, line2, true, false);
            if (point != null && !point.OnLine(line1)) { inverted = true; return (line2, line1); }

            return (null, null);
        }
      
    }
}
