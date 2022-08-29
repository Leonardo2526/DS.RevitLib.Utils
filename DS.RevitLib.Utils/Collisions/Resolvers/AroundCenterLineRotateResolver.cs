using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    internal class AroundCenterLineRotateResolver : CollisionResolver<TransformModel>
    {
        private readonly SolidModelExt _operationElement;
        private readonly RotationModel _rotationModel;

        public AroundCenterLineRotateResolver(Collision<SolidModelExt, Element> collision, 
            ICollisionChecker collisionChecker, TransformModel transformModel) :
            base(collision, collisionChecker)
        {
            TransformModel = transformModel;
            _rotationModel = transformModel.AroundCenterLineRotation;
            _operationElement = collision.Object1;
        }

        public TransformModel TransformModel { get; private set; }



        public override TransformModel Resolve()
        {
            XYZ axis = _rotationModel.Axis is null ?
                _operationElement.CentralLine.Direction : _rotationModel.Axis.Direction;

            double angle = ResolvePosition(axis, _operationElement.CentralPoint);
            if (angle != 0)
            {
                TransformModel.AroundCenterLineRotation = new RotationModel(_operationElement.CentralLine, angle);
            }
            else
            {
                _successor.Resolve();
            }

            Solution = TransformModel;
            return TransformModel;
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
                    return angle;
                }
            }

            return 0;
        }
    }
}
