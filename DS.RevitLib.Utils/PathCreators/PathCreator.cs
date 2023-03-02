using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.PathCreators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.PathCreators
{

    /// <summary>
    ///  Object representing a tool to create path between points without collisions account.
    /// </summary>
    public class PathCreator : IPathCreator
    {
        private XYZ _point1;
        private XYZ _point2;
        private Line _baseLine1;
        private  Line _baseLine2;
        private double _minPointDist;
        private double _minZDist;
        private double _angle;
        private double _maxCutWidth;

        /// <inheritdoc/>
        public IPathCreator Create(Line baseLine1, Line baseLine2 = null,
            double minPointDist = 0, double maxCutWidth = 0, double angle = 90, double minZDist = 0)
        {
            if (minZDist != 0 && minZDist < minPointDist)
            {
                throw new ArgumentException("minZDist < minPointDist");
            }

            _baseLine1 = baseLine1;
            _baseLine2 = baseLine2;
            _minPointDist = minPointDist;
            _maxCutWidth = maxCutWidth;
            _minZDist = minZDist;
            _angle = angle;

            return this;
        }


        /// <summary>
        /// Create path between two points.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns>Returns path's coordinates.</returns>
        public List<XYZ> Create(XYZ point1, XYZ point2)
        {
            _point1 = point1;
            _point2 = point2;

            _baseLine1 ??= Line.CreateBound(point1, point2);

            var result = new List<XYZ>() { _point1 };

            XYZ gp1 = GetGp1(out Line line1);

            var line2 = Line.CreateUnbound(gp1, _baseLine1.Direction);
            XYZ gp2 = line2.Project(_point2).XYZPoint;

            var line3 = Line.CreateUnbound(gp2, line2.Direction.CrossProduct(gp1 - _point1));
            XYZ gp3 = line3.Project(point2).XYZPoint;

            if (Math.Round(gp1.DistanceTo(_point1), 3) >= _minPointDist)
            {
                result.Add(gp1);
            }
            if (Math.Round(gp2.DistanceTo(_point2), 3) >= _minPointDist && Math.Round(gp2.DistanceTo(gp1), 3) >= _minPointDist)
            {
                result.Add(gp2);

            }
            if (Math.Round(gp3.DistanceTo(_point2), 3) >= _minPointDist && Math.Round(gp3.DistanceTo(gp2), 3) >= _minPointDist)
            {
                result.Add(gp3);
            }

            result.Add(_point2);

            result = CutAngles(result);

            return result;
        }


        #region PrivateProperties

        private XYZ GetGp1(out Line line)
        {
            XYZ dir1 = _baseLine1.Direction;
            XYZ dir2 = _baseLine2?.Direction;
            XYZ dirZ;
            if (dir2 is null || XYZUtils.Collinearity(dir1, dir2))
            {
                dirZ = _baseLine1.Direction.CrossProduct(XYZ.BasisX);
                dirZ = dirZ.IsZeroLength() ? _baseLine1.Direction.CrossProduct(XYZ.BasisY) : dirZ;
            }
            else
            {
                dirZ = _baseLine1.Direction.CrossProduct(dir2);
            }

            dirZ = dirZ.Normalize();
            line = Line.CreateUnbound(_point1, dirZ);
            XYZ gp1 = line.Project(_point2).XYZPoint;
            if (gp1.DistanceTo(_point1) < _minZDist)
            {
                gp1 = _point1 + dirZ.Multiply(_minZDist);
            }
            if (gp1.DistanceTo(_point1) < _minPointDist)
            {
                gp1 = _point1 + dirZ.Multiply(_minPointDist);
            }
            return gp1;
        }

        private (XYZ p1, XYZ p2) CutAngle(XYZ anglePoint, XYZ startPoint, XYZ endPoint, double cutWidth)
        {
            Line startLine = Line.CreateBound(startPoint, anglePoint);
            Line endLine = Line.CreateBound(anglePoint, endPoint);
            Line startLineUnbLine = Line.CreateUnbound(startLine.Origin, startLine.Direction);

            XYZ moveDir = (startPoint - anglePoint).Normalize();
            XYZ moveVector = moveDir.Multiply(cutWidth);
            Transform move = Transform.CreateTranslation(moveVector);

            XYZ rotAxis = startLine.Direction.CrossProduct(endLine.Direction).Negate();
            Transform rotate = Transform.CreateRotationAtPoint(rotAxis, _angle.DegToRad(), anglePoint + moveVector);

            //commit transforms to line
            var operationCurve = endLine.CreateTransformed(move);
            operationCurve = operationCurve.CreateTransformed(rotate);

            //show
            //var l = operationCurve as Line;
            //l = l.IncreaseLength(10);
            //l.Show(_uIDocument.Document);
            //startLine.Show(_uIDocument.Document);
            //endLine.Show(_uIDocument.Document);
            //_uIDocument.RefreshActiveView();

            var intersectionStart = operationCurve.Intersect(startLine, out IntersectionResultArray intersectionArrayStart);
            var intersectionEnd = operationCurve.Intersect(endLine, out IntersectionResultArray intersectionArrayEnd);

            var startPointRes = intersectionArrayStart?.Cast<IntersectionResult>().First().XYZPoint;
            var endPointRes = intersectionArrayEnd?.Cast<IntersectionResult>().First().XYZPoint;

            return (startPointRes, endPointRes);
        }

        private List<XYZ> CutAngles(List<XYZ> points)
        {
            if (_angle == 90 | points.Count < 3)
            {
                return points;
            }

            var cutPoints = new List<XYZ>() { points[0] };
            for (int i = 1; i < points.Count - 1; i++)
            {
                double cutWidth = GetCutWidth(cutPoints.Last(), points[i], points[i + 1]);
                var (p1, p2) = CutAngle(points[i], cutPoints.Last(), points[i + 1], cutWidth);

                if (p1 is not null && !ContainsPoint(cutPoints, p1))
                {
                    cutPoints.Add(p1);
                }

                if (p2 is not null && !ContainsPoint(cutPoints, p2))
                {
                    cutPoints.Add(p2);
                }

            }

            cutPoints.Add(points.Last());

            return cutPoints;
        }

        private double GetCutWidth(XYZ startPoint, XYZ anglePoint, XYZ endPoint)
        {
            double cutWidth;
            if ((startPoint - _point1).IsZeroLength())
            {
                cutWidth = anglePoint.DistanceTo(_point1);
                return cutWidth > _maxCutWidth ? _maxCutWidth : cutWidth;
            }
            else if ((endPoint - _point2).IsZeroLength())
            {
                cutWidth = anglePoint.DistanceTo(_point2);
                return cutWidth > _maxCutWidth ? _maxCutWidth : cutWidth;
            }



            double leg1 = anglePoint.DistanceTo(startPoint) - _minPointDist;
            double leg2 = anglePoint.DistanceTo(endPoint) - _minPointDist;

            cutWidth = leg1 > leg2 ? leg2 : leg1;
            cutWidth = cutWidth > _maxCutWidth ? _maxCutWidth : cutWidth;
            return cutWidth;
        }

        private bool ContainsPoint(List<XYZ> points, XYZ point)
        {
            foreach (var p in points)
            {
                if ((p - point).IsZeroLength())
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
