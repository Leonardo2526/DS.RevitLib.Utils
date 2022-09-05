using Autodesk.Revit.DB;
using DS.RevitLib.Test;
using DS.RevitLib.Test.ElementTransferTest;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.Visualisators;
using System.Reflection;
using DS.RevitLib.Utils.ModelCurveUtils;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    internal class MoveResolver : CollisionResolver
    {
        private readonly SolidModelExt _operationElement;
        private readonly XYZ _basePoint;
        private readonly TargetMEPCuve _targetMEPCurve;
        private readonly Solid _totalIntersectionSolid;
        private readonly double _minDistBetweenElems = 50.mmToFyt2();

        public MoveResolver(SolidElemCollision collision, ICollisionChecker collisionChecker,
            XYZ basePoint, TargetMEPCuve targetMEPCurve, Solid totalIntersectionSolid) :
            base(collision, collisionChecker)
        {
            _operationElement = collision.Object1;
            _basePoint = basePoint;
            _targetMEPCurve = targetMEPCurve;
            _totalIntersectionSolid = totalIntersectionSolid;
            MovePoint = _basePoint;
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
            IsResolved = true;

            if (!_collisionChecker.GetCollisions().Any())
            {
                IsResolved = true;
            }
            else
            {
                _successor.Resolve();
            }
        }

        private XYZ GetPoint()
        {
            Line line = _operationElement.CentralLine.IncreaseLength(10);
            double moveLength = GetMoveLength(_totalIntersectionSolid, line);
            XYZ point = _basePoint + _targetMEPCurve.Direction.Multiply(moveLength);
            if (point.IsBetweenPoints(_targetMEPCurve.StartPlacementPoint, _targetMEPCurve.EntPlacementPoint))
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
            if (!vector.IsAlmostEqualTo(_targetMEPCurve.Direction))
            {
                opPoints.Reverse();
            }
            //ShowPoints(opPoint1, opPoint2);

            XYZ edgePoint = XYZUtils.GetClosestToPoint(opPoints.Last(), totalSolidPoints);
            double edgeDist = edgePoint.DistanceTo(opPoints.First());
            //ShowPoints(edgePoint, opPoints.First());

            return edgeDist + _minDistBetweenElems;
        }


        private void Show(Solid solid)
        {
            BoundingBoxXYZ box = solid.GetBoundingBox();
            IVisualisator vs = new BoundingBoxVisualisator(box, _operationElement.Element.Document);
            new Visualisator(vs);
        }

        private void ShowPoints(XYZ point1, XYZ point2)
        {
            ModelCurveCreator modelCurveCreator = new ModelCurveCreator(_operationElement.Element.Document);
            modelCurveCreator.Create(point1, point2);
        }
    }
}
