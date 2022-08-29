using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    internal class AroundCenterLineRotateResolver : AbstractCollisionResolver
    {
        private readonly SolidModelExt _operationElement;
        private readonly RotationModel _rotationModel;

        public AroundCenterLineRotateResolver(ICollisionChecker collisionChecker,
            SolidModelExt operationElement, TransformModel transformModel) : base(collisionChecker)
        {
            _operationElement = operationElement;
            TransformModel = transformModel;
        }

        public TransformModel TransformModel { get; private set; }



        public override void Resolve()
        {
            XYZ axis = _rotationModel.Axis is null ?
                _operationElement.CentralLine.Direction : _rotationModel.Axis.Direction;

            double angle = ResolvePosition(axis, _operationElement.CentralPoint);
            if (IsResolved)
            {
                TransformModel.AroundCenterLineRotation = new RotationModel(
                    Line.CreateUnbound(_operationElement.CentralPoint, _operationElement.CentralPoint + axis), angle);
            }
            else
            {
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
