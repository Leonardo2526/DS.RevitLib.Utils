using Autodesk.Revit.DB;

using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Elements.Transfer.Resolvers;
using System.Collections.Generic;
using System.Linq;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Extensions;

namespace DS.RevitLib.Utils.Elements.Transfer
{
    internal class SolidCollisionClient
    {
        private readonly SolidModelExt _operationElement;
        private readonly List<(Solid, Element)> _collisions;
        private readonly ISolidCollisionDetector _detector;
        private readonly TargetPlacementModel _targetModel;
        private XYZ _currentPoint;
        private readonly double _minCurveLength;
        private readonly List<Element> _excludedElements;

        public SolidCollisionClient(SolidModelExt operationElement, List<(Solid, Element)> collisions, ISolidCollisionDetector collisionChecker,
            TargetPlacementModel targetModel, double minCurveLength, List<Element> excludedElements)
        {
            _operationElement = operationElement;
            _collisions = collisions;
            _detector = collisionChecker;
            _targetModel = targetModel;
            _currentPoint = targetModel.StartPlacementPoint;
            _minCurveLength = minCurveLength;
            _excludedElements = excludedElements;
        }


        public void Resolve()
        {
            var collision = _collisions.First();

            var clr = new RotateCenterLineResolver(_operationElement, collision, _detector, _excludedElements);

            //check profile
            (double width, double heigth) = collision.Item2 is MEPCurve ?
                MEPCurveUtils.GetWidthHeight(collision.Item2 as MEPCurve) : 
                (0,0);
            var aclr = width==heigth ? new AroundCenterLineRotateResolver(_operationElement, collision, _detector, _excludedElements) : null;

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
                    MoveResolver(_operationElement, collision, _detector, _currentPoint, _targetModel, totalIntersectionSolid, _minCurveLength, _excludedElements);
                aclr = aclr is null ? null : new AroundCenterLineRotateResolver(_operationElement, collision, _detector, _excludedElements);
                clr = new RotateCenterLineResolver(_operationElement, collision, _detector, _excludedElements);

                var currentCollisions = new List<(Solid, Element)>();
                currentCollisions.AddRange(_collisions);
                while (!mr.IsResolved)
                {
                    totalIntersectionSolid = GetIntersectionSolid(currentCollisions);
                    if (totalIntersectionSolid is null)
                    {
                        return;
                    }
                    mr = new MoveResolver(_operationElement, collision, _detector, _currentPoint, _targetModel, totalIntersectionSolid, _minCurveLength, _excludedElements);
                    aclr = aclr is null ? null : new AroundCenterLineRotateResolver(_operationElement, collision, _detector, _excludedElements);
                    clr = new RotateCenterLineResolver(_operationElement, collision, _detector, _excludedElements);

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
                        collision = mr.UnresolvedCollisions.First();
                        currentCollisions = mr.UnresolvedCollisions;
                    }
                }
            }
        }

        private Solid GetIntersectionSolid(List<(Solid, Element)> collisions)
        {
            var solid = collisions.Select(obj => obj.GetIntersectionSolid(obj.Item2.Document)).ToList();
            return DS.RevitLib.Utils.Solids.SolidUtils.UniteSolids(solid);
        }
    }
}
