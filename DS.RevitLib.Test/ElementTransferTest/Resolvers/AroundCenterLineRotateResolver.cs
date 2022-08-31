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
    internal class AroundCenterLineRotateResolver : CollisionResolver
    {
        private readonly SolidModelExt _operationElement;
        private int _successorUsageCount = 0;

        public AroundCenterLineRotateResolver(Collision<SolidModelExt, Element> collision, 
            ICollisionChecker collisionChecker) :
            base(collision, collisionChecker)
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
                if (_successorUsageCount >0)
                {
                    return;
                }
                _successorUsageCount++;
                _successor.Resolve();
            }
        }

        public void ResolvePosition(XYZ axis, XYZ point)
        {
            for (int i = 0; i < 3; i++)
            {
                double angle = +90.DegToRad();
                Transform rotateTransform = Transform.CreateRotationAtPoint(axis, angle, point);
                _operationElement.Transform(rotateTransform);

                if (!_collisionChecker.GetCollisions().Any())
                {
                    IsResolved = true;
                    break;
                }
            }
        }
    }
}
