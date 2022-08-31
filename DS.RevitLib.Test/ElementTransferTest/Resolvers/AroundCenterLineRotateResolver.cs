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
        private readonly RotationModel _rotationModel;
        private int _successorUsageCount = 0;

        public AroundCenterLineRotateResolver(Collision<SolidModelExt, Element> collision, 
            ICollisionChecker collisionChecker) :
            base(collision, collisionChecker)
        {
            _rotationModel = collision.Object1.TransformModel.MaxOrthLineRotation;
            _operationElement = collision.Object1;
        }


        public override void Resolve()
        {
            XYZ axis = _rotationModel.Axis is null ?
                _operationElement.CentralLine.Direction : _rotationModel.Axis.Direction;

            double angle = ResolvePosition(axis, _operationElement.CentralPoint);
                _operationElement.TransformModel.MaxOrthLineRotation = new RotationModel(_operationElement.CentralLine, angle);
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

        public double ResolvePosition(XYZ axis, XYZ point)
        {
            for (int i = 0; i < 3; i++)
            {
                double angle = +90.DegToRad();
                Transform rotateTransform = Transform.CreateRotationAtPoint(axis, angle, point);
                _operationElement.Transform(rotateTransform);

                if (!_collisionChecker.GetCollisions().Any())
                {
                    IsResolved = true;
                    return angle;
                }
            }

            return 0;
        }
    }
}
