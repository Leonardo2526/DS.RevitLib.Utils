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
    internal class RotateCenterLineResolver : CollisionResolver<TransformModel>
    {
        private readonly SolidModelExt _operationElement;
        private readonly RotationModel _rotationModel;

        public RotateCenterLineResolver(Collision<SolidModelExt, Element> collision, ICollisionChecker collisionChecker) : 
            base(collision, collisionChecker)
        {
            _rotationModel = collision.Object1.TransformModel.CenterLineRotation;
            _operationElement = collision.Object1;
        }

        public override TransformModel Resolve()
        {
            XYZ axis = _rotationModel.Axis is null ?
             _operationElement.MaxOrth : _rotationModel.Axis.Direction;

            Line axisLine = Line.CreateBound(_operationElement.CentralPoint, _operationElement.CentralPoint + axis);
            double angle = 180.DegToRad();
            _operationElement.TransformModel.CenterLineRotation = new RotationModel(axisLine, angle);
            if (!_collisionChecker.GetCollisions().Any())
            {
                IsResolved = true;
                return _operationElement.TransformModel;
            }
            else
            {
                _successor.Resolve();
            }

            return _operationElement.TransformModel;
        }
    }
}
