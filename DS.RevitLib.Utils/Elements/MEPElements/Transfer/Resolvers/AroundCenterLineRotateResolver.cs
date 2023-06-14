using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Solids.Models;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements.Transfer.Resolvers
{
    internal class AroundCenterLineRotateResolver : CollisionResolver
    {
        private readonly SolidModelExt _operationElement;
        private int _successorUsageCount = 0;

        public AroundCenterLineRotateResolver(Collision<SolidModelExt, Element> collision,
            List<ICollisionChecker> collisionCheckers) :
            base(collision, collisionCheckers)
        {
            _operationElement = collision.Object1;
        }


        public override void Resolve()
        {
            XYZ axis = _operationElement.CentralLine.Direction;
            ResolvePosition(axis, _operationElement.CentralPoint);

            if (IsResolved)
            {
                return;
            }
            else
            {
                if (_successorUsageCount > 0)
                {
                    return;
                }
                _successorUsageCount++;
                _successor.Resolve();
                if (_successor.UnresolvedCollisions.Any())
                {
                    UnresolvedCollisions = _successor.UnresolvedCollisions;
                }
            }
        }

        public void ResolvePosition(XYZ axis, XYZ point)
        {
            for (int i = 0; i < 3; i++)
            {
                double angle = +90.DegToRad();
                Transform rotateTransform = Transform.CreateRotationAtPoint(axis, angle, point);
                _operationElement.Transform(rotateTransform);

                UnresolvedCollisions = GetCollisions();

                if (!UnresolvedCollisions.Any())
                {
                    IsResolved = true;
                    break;
                }
            }
        }
    }
}
