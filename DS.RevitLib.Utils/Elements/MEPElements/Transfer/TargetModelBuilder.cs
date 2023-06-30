using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Elements.Transfer
{
    internal class TargetModelBuilder
    {
        private readonly LineModel _lineModel;
        private readonly double _placementLength;
        private readonly List<XYZ> _points;
        private readonly XYZ _p1;
        private readonly XYZ _p2;

        public TargetModelBuilder(LineModel lineModel, double placementLength, List<XYZ> points)
        {
            _lineModel = lineModel;
            _placementLength = placementLength;
            _points = points;
            _p1 = _lineModel.Line.GetEndPoint(0);
            _p2 = _lineModel.Line.GetEndPoint(1);
        }

        public TargetPlacementModel Build()
        {
            XYZ basePoint = GetBasePoint();
            XYZ startPlacementPoint = basePoint is null
                ? new PlacementPoint(_lineModel.Line, _placementLength).GetPoint(PlacementOption.Edge)
                : new PlacementPoint(_lineModel.Line, _placementLength).GetPoint(basePoint);

            XYZ endPoint = startPlacementPoint.DistanceTo(_p1) > startPlacementPoint.DistanceTo(_p2) ?
                _p1 : _p2;
            XYZ startPoint = Math.Round(endPoint.DistanceTo(_p1), 3) == 0 ?
                _p2 : _p1;

            XYZ vector = (endPoint - startPlacementPoint).RoundVector().Normalize();
            XYZ endPlacementPoint = endPoint - vector.Multiply(_placementLength / 2);

            return new TargetPlacementModel(_lineModel, startPlacementPoint, endPlacementPoint, startPoint, endPoint);
        }

        private XYZ GetBasePoint()
        {
            for (int i = 0; i < _points.Count - 1; i++)
            {
                //check if points conicidence
                if (Math.Round(_points[i].DistanceTo(_p1), 3) == 0 | Math.Round(_points[i].DistanceTo(_p2), 3) == 0)
                {
                    return _points[i];
                }

                if (_p1.IsBetweenPoints(_points[i], _points[i + 1]) | _p2.IsBetweenPoints(_points[i], _points[i + 1]))
                {
                    if (_points[i].DistanceTo(_p1) > _points[i].DistanceTo(_p2))
                    {
                        return _p2;
                    }
                    else
                    { return _p1; }
                }
            }

            return null;
        }
    }
}
