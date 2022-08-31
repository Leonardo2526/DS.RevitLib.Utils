using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.ElementTransferTest
{
    internal class TransformBuilder
    {
        private readonly SolidModelExt _sorceModel;
        private readonly SolidModelExt _operationModel;

        public TransformBuilder(SolidModelExt sorceModel, SolidModelExt operationModel)
        {
            _sorceModel = sorceModel;
            _operationModel = operationModel;
        }

        public TransformModel Build()
        {
            var transformModel = new TransformModel();

            XYZ moveVector = _operationModel.CentralPoint - _sorceModel.CentralPoint;
            if (!moveVector.IsZeroLength())
            {
                transformModel.MoveVector = moveVector;
            }

            //get centerline rotation model
            XYZ sourceDir = _sorceModel.CentralLine.Direction;
            XYZ opDir = _operationModel.CentralLine.Direction;
            XYZ axisDir = sourceDir.CrossProduct(opDir).RoundVector().Normalize();
            Line axis;
            if (axisDir.IsZeroLength())
            {
                if (sourceDir.IsAlmostEqualTo(opDir.Negate(),3))
                {
                    double angle = Math.Round(sourceDir.AngleTo(opDir), 3);
                    axis = Line.CreateBound(_operationModel.CentralPoint, _operationModel.CentralPoint + XYZ.BasisZ);
                    transformModel.CenterLineRotation = new RotationModel(axis, angle);
                }
            }
            else
            {
                axis = Line.CreateBound(_operationModel.CentralPoint, _operationModel.CentralPoint + axisDir);
                transformModel.CenterLineRotation = GetRotationModel(sourceDir, opDir, axis);
            }

            //get maxOrth rotation model
            sourceDir = transformModel.CenterLineRotation is null ? _sorceModel.MaxOrthLine.Direction :
                GetRotated(_sorceModel.MaxOrthLine, transformModel.CenterLineRotation.Axis.Direction, transformModel.CenterLineRotation.Angle).
                Direction.RoundVector().Normalize();
            opDir = _operationModel.MaxOrthLine.Direction;
            axis = _operationModel.CentralLine;
            transformModel.MaxOrthLineRotation = GetRotationModel(sourceDir, opDir, axis);


            return transformModel;
        }

        private RotationModel GetRotationModel(XYZ sourceDir, XYZ opDir, Line axis)
        {
            if (XYZUtils.Collinearity(sourceDir, opDir))
            {
                return null;
            }

            double angle = Math.Round(sourceDir.AngleTo(opDir), 3);
            if (!XYZUtils.BasisEqualToOrigin(sourceDir, opDir, axis.Direction))
            {
                angle = -angle;
            }
            return new RotationModel(axis, angle);

        }


        private Line GetRotated(Line line, XYZ axis, double angle)
        {
            Transform rotateTransform = Transform.CreateRotation(axis, angle);
            return line.CreateTransformed(rotateTransform) as Line;
        }
    }
}
