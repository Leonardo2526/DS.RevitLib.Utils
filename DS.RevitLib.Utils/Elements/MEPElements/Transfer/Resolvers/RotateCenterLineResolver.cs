using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Solids.Models;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements.Transfer.Resolvers
{
    internal class RotateCenterLineResolver : CollisionResolver
    {
        private readonly SolidModelExt _operationElement;

        public RotateCenterLineResolver(Collision<SolidModelExt, Element> collision, List<ICollisionChecker> collisionCheckers) :
            base(collision, collisionCheckers)
        {
            _operationElement = collision.Object1;
        }

        public override void Resolve()
        {
            XYZ axis = _operationElement.Basis.Y;
            double angle = 180.DegToRad();

            Transform rotateTransform = Transform.CreateRotationAtPoint(axis, angle, _operationElement.CentralPoint);
            _operationElement.Transform(rotateTransform);

            UnresolvedCollisions = GetCollisions();

            if (!UnresolvedCollisions.Any())
            {
                IsResolved = true;
            }
            else
            {
                _successor?.Resolve();
                if (_successor?.UnresolvedCollisions is not null && _successor.UnresolvedCollisions.Any())
                {
                    UnresolvedCollisions = _successor.UnresolvedCollisions;
                }
            }
        }
    }
}
