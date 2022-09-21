using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.PathFinders;
using System;
using System.Collections.Generic;

namespace DS.RevitApp.Test.PathFinders
{
    public class SimplePathFinder : IPathFinder
    {

        private XYZ _point1;
        private XYZ _point2;
        private Line _baseLine1;
        private readonly Line _baseLine2;
        private readonly double _minPointDist;
        private readonly double _minZDist;

        public SimplePathFinder(Line baseLine1, Line baseLine2 = null, double minPointDist = 0, double minZDist = 0)
        {
            if (minZDist < minPointDist)
            {
                throw new ArgumentException("minZDist < minPointDist");
            }
            _baseLine1 = baseLine1;
            _baseLine2 = baseLine2;
            _minPointDist = minPointDist;
            _minZDist = minZDist;
        }

        public List<XYZ> Find(XYZ point1, XYZ point2)
        {
            _point1 = point1;
            _point2 = point2;

            _baseLine1 ??= Line.CreateBound(point1, point2);

            var result = new List<XYZ>() { _point1 };

            XYZ dir1 = _baseLine1.Direction;
            XYZ dir2 = _baseLine2?.Direction;
            XYZ dirZ;
            if (XYZUtils.Collinearity(dir1, dir2))
            {
                dirZ = _baseLine1.Direction.CrossProduct(XYZ.BasisX);
                dirZ = dirZ.IsZeroLength() ? _baseLine1.Direction.CrossProduct(XYZ.BasisY) : dirZ;
            }
            else
            {
                dirZ = _baseLine1.Direction.CrossProduct(dir2);
            }

            Line line = Line.CreateUnbound(point1, dirZ);
            XYZ gp1 = line.Project(point2).XYZPoint;
            if (gp1.DistanceTo(_point1) < _minZDist)
            {
                gp1 += dirZ.Multiply(_minZDist);
            }

            line = Line.CreateUnbound(gp1, _baseLine1.Direction);
            XYZ gp2 = line.Project(_point2).XYZPoint;

            line = Line.CreateUnbound(gp2, line.Direction.CrossProduct(gp1 - _point1));
            XYZ gp3 = line.Project(point2).XYZPoint;

            if (Math.Round(gp1.DistanceTo(_point1), 3) != 0)
            {
                result.Add(gp1);
            }
            if (Math.Round(gp2.DistanceTo(_point2), 3) != 0 && Math.Round(gp2.DistanceTo(gp1), 3) != 0)
            {
                result.Add(gp2);

            }
            if (Math.Round(gp3.DistanceTo(_point2), 3) != 0 && Math.Round(gp3.DistanceTo(gp2), 3) != 0)
            {
                result.Add(gp3);
            }

            result.Add(_point2);

            return result;
        }
    }
}
