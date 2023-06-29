using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Elements.Transfer.Resolvers;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements.Transfer
{
    internal class SolidCollisionClient
    {
        private readonly List<SolidElemTransformCollision> _collisions;
        private readonly List<ICollisionChecker> _collisionCheckers;
        private readonly TargetPlacementModel _targetModel;
        private XYZ _currentPoint;
        private readonly double _minCurveLength;


        public SolidCollisionClient(List<SolidElemTransformCollision> collisions, List<ICollisionChecker> collisionChecker,
            TargetPlacementModel targetModel, double minCurveLength)
        {
            _collisions = collisions;
            _collisionCheckers = collisionChecker;
            _targetModel = targetModel;
            _currentPoint = targetModel.StartPlacementPoint;
            _minCurveLength = minCurveLength;
        }


        public void Resolve()
        {
            var collision = _collisions.First();

            var clr = new RotateCenterLineResolver(collision, _collisionCheckers);

            //check profile
            (double width, double heigth) = collision.Object2 is MEPCurve ?
                MEPCurveUtils.GetWidthHeight(collision.Object2 as MEPCurve) : 
                (0,0);
            var aclr = width==heigth ? new AroundCenterLineRotateResolver(collision, _collisionCheckers) : null;

            if (aclr is null)
            {
                clr.Resolve();
            }
            else
            {
                clr.SetSuccessor(aclr); // if not resolved, rotate around center line.
                aclr.SetSuccessor(clr); // if not resolved, rotate center line at 180 degeres.
                clr.Resolve();
            }

            if (!clr.IsResolved)
            {
                Solid totalIntersectionSolid = GetIntersectionSolid(_collisions);
                var mr = new DS.RevitLib.Utils.Elements.Transfer.Resolvers.
                    MoveResolver(collision, _collisionCheckers, _currentPoint, _targetModel, totalIntersectionSolid, _minCurveLength);
                aclr = aclr is null ? null : new AroundCenterLineRotateResolver(collision, _collisionCheckers);
                clr = new RotateCenterLineResolver(collision, _collisionCheckers);

                var currentCollisions = new List<SolidElemTransformCollision>();
                currentCollisions.AddRange(_collisions);
                while (!mr.IsResolved)
                {
                    totalIntersectionSolid = GetIntersectionSolid(currentCollisions);
                    if (totalIntersectionSolid is null)
                    {
                        return;
                    }
                    mr = new DS.RevitLib.Utils.Elements.Transfer.Resolvers.
                        MoveResolver(collision, _collisionCheckers, _currentPoint, _targetModel, totalIntersectionSolid, _minCurveLength);
                    aclr = aclr is null ? null : new AroundCenterLineRotateResolver(collision, _collisionCheckers);
                    clr = new RotateCenterLineResolver(collision, _collisionCheckers);

                    mr.SetSuccessor(clr);
                   
                    if(aclr is not null) 
                    { 
                        clr.SetSuccessor(aclr); // if not resolved, rotate around center line.
                        aclr.SetSuccessor(clr); // if not resolved, rotate center line at 180 degeres.
                    } 
                    mr.Resolve();
                    _currentPoint = mr.MovePoint;
                    if (_currentPoint is null)
                    {
                        return;
                    }

                    if (mr.UnresolvedCollisions is not null && mr.UnresolvedCollisions.Any())
                    {
                        collision = (SolidElemTransformCollision)mr.UnresolvedCollisions.First();
                        currentCollisions = mr.UnresolvedCollisions.Cast<SolidElemTransformCollision>().ToList();
                    }
                }
            }
        }

        private Solid GetIntersectionSolid(List<SolidElemTransformCollision> collisions)
        {
            var solid = collisions.Select(obj => obj.GetIntersection()).ToList();
            return DS.RevitLib.Utils.Solids.SolidUtils.UniteSolids(solid);
        }
    }
}
