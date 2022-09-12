using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Collisions.Resolvers;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.Collisions.Resolvers
{
    internal class RotateCenterLineResolver : CollisionResolver
    {
        private readonly SolidModelExt _operationElement;

        public RotateCenterLineResolver(Collision<SolidModelExt, Element> collision, ICollisionChecker collisionChecker) : 
            base(collision, collisionChecker)
        {
            _operationElement = collision.Object1;
        }

        public override void Resolve()
        {
            XYZ axis = _operationElement.Basis.Y;
            double angle = 180.DegToRad();

            Transform rotateTransform = Transform.CreateRotationAtPoint(axis, angle, _operationElement.CentralPoint);
            _operationElement.Transform(rotateTransform);

            if (!_collisionCheckers.First().GetCollisions().Any())
            {
                IsResolved = true;
            }
            else
            {
                _successor.Resolve();
            }
        }
    }
}
