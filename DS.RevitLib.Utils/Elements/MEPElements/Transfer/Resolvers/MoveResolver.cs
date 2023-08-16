using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements.Transfer.Resolvers
{
    internal class MoveResolver : CollisionResolver
    {
        private readonly XYZ _basePoint;
        private readonly TargetPlacementModel _targetModel;
        private readonly Solid _totalIntersectionSolid;
        private readonly double _minCurveLength;

        public MoveResolver(SolidModelExt operationElement, SolidElementCollision collision, ISolidCollisionDetector detector,
            XYZ basePoint, TargetPlacementModel targetModel, Solid totalIntersectionSolid, double minCurveLength, List<Element> excludedElements) :
            base(operationElement, collision, detector, excludedElements)
        {
            _basePoint = basePoint;
            _targetModel = targetModel;
            _totalIntersectionSolid = totalIntersectionSolid;
            MovePoint = _basePoint;
            _minCurveLength = minCurveLength;
        }

        public XYZ MovePoint { get; private set; }

        public override void Resolve()
        {
            MovePoint = GetPoint();
            if (MovePoint is null)
            {
                return;
            }

            XYZ moveVector = MovePoint - _operationElement.CentralPoint;
            Transform rotateTransform = Transform.CreateTranslation(moveVector);
            _operationElement.Transform(rotateTransform);

            UnresolvedCollisions = _detector.GetCollisions(_operationElement.Solid);

            if (!UnresolvedCollisions.Any())
            {
                IsResolved = true;
            }
            else
            {
                _successor.Resolve();
                if (_successor.UnresolvedCollisions.Any())
                {
                    UnresolvedCollisions = _successor.UnresolvedCollisions;
                }
            }
        }

        private XYZ GetPoint()
        {
            Line line = _operationElement.CentralLine.IncreaseLength(10);
            double moveLength = GetMoveLength(_totalIntersectionSolid, line);
            XYZ point = _basePoint + _targetModel.LineModel.Line.Direction.Multiply(moveLength);
            if (point.IsBetweenPoints(_targetModel.StartPlacementPoint, _targetModel.EndPlacementPoint))
            {
                return point;
            }

            return null;
        }


        private (XYZ point1, XYZ point2) GetEdgeProjectPoints(Solid solid, Line line)
        {
            List<XYZ> solidPoints = solid.ExtractPoints();
            List<XYZ> projSolidPoints = solidPoints.Select(obj => line.Project(obj).XYZPoint).ToList();
            (XYZ point1, XYZ point2) = XYZUtils.GetMaxDistancePoints(projSolidPoints, out double dist);
            return (point1, point2);
        }

        private double GetMoveLength(Solid solid, Line line)
        {
            (XYZ point1, XYZ point2) = GetEdgeProjectPoints(solid, line);
            var totalSolidPoints = new List<XYZ>()
            {
                point1, point2
            };
            double totalSolidLength = point1.DistanceTo(point2);
            //Show(solid);
            //ShowPoints(point1, point2);

            (XYZ opPoint1, XYZ opPoint2) = GetEdgeProjectPoints(_operationElement.Solid, line);
            XYZ vector = (opPoint2 - opPoint1).RoundVector().Normalize();
            var opPoints = new List<XYZ>()
            {
                opPoint1, opPoint2
            };
            if (!vector.IsAlmostEqualTo(_targetModel.LineModel.Line.Direction))
            {
                opPoints.Reverse();
            }
            //ShowPoints(opPoint1, opPoint2);

            XYZ edgePoint = XYZUtils.GetClosestToPoint(opPoints.Last(), totalSolidPoints);
            double edgeDist = edgePoint.DistanceTo(opPoints.First());
            //ShowPoints(edgePoint, opPoints.First());

            return edgeDist + _minCurveLength;
        }

    }
}
