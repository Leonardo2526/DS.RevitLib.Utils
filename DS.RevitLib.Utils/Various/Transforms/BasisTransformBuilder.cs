using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Transforms;
//using OLMP.MEPAC.Entities.LogMessage;
//using Revit.Async;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Transforms
{
    public class BasisTransformBuilder : TransformBuilder
    {
        private readonly Basis _operationObject;

        public BasisTransformBuilder(Basis sourceObject, Basis targetObject) : base(sourceObject, targetObject)
        {
            _operationObject = sourceObject.Clone();
        }

        /// <summary>
        /// Build transform to align two baseses
        /// </summary>
        /// <returns></returns>
        public override TransformModel Build()
        {
            var target = _targetObject as Basis;
            var source = _sourceObject as Basis;

            var transformModel = new BasisTransformModel(_operationObject, target);

            transformModel.MoveVector = target.Point - _operationObject.Point;
            Transform transform = Transform.CreateTranslation(transformModel.MoveVector);
            _operationObject.Transform(transform);
            transformModel.Transforms.Add(transform);

            if (!_operationObject.IsOrthogonal() | !target.IsOrthogonal())
            {
                string errors = "Basisis are not orthogonal.";
                throw new InvalidOperationException(errors);
            }
            bool targetOrthogonality = target.IsOrthogonal();
            if (_operationObject.GetOrientaion() != source.GetOrientaion())
            {
                string errors = "Orientaions are not equal.";
                throw new InvalidOperationException(errors);
            }

            int i = 0;
            (XYZ basis1, XYZ basis2) = GetNotEqualBasises(_operationObject, target);
            while (basis1 is not null && i < 3)
            {
                double angle;
                XYZ axis = basis1.CrossProduct(basis2).RoundVector();
                if (axis.IsZeroLength())
                {
                    angle = 180.DegToRad();
                    axis = XYZUtils.GetPerpendicular(basis1,
                        new List<XYZ>() { _operationObject.X, _operationObject.Y, _operationObject.Z }).First();
                }
                else
                {
                    angle = basis1.AngleTo(basis2);
                }
                transform = Transform.CreateRotationAtPoint(axis, angle, _operationObject.Point);
                _operationObject.Transform(transform);
                transformModel.Transforms.Add(transform);

                Line axisLine = Line.CreateBound(_operationObject.Point, _operationObject.Point + axis);
                transformModel.Rotations.Add(new RotationModel(axisLine, angle));
                (basis1, basis2) = GetNotEqualBasises(_operationObject, target);
                i++;
            }
            if (i > 2)
            {
                string errors = "Failed to get transform model: number of rotation steps > 2";
                throw new InvalidOperationException(errors);
            }


            return transformModel;
        }

        private (XYZ basis1, XYZ basis2) GetNotEqualBasises(Basis basis1, Basis basis2)
        {
            double angleTolerance = 3.DegToRad();

            List<XYZ> basises1 = new List<XYZ>()
            {
                basis1.X, basis1.Y, basis1.Z
            };
            List<XYZ> basises2 = new List<XYZ>()
            {
                basis2.X, basis2.Y, basis2.Z
            };

            for (int i = 0; i < 3; i++)
            {
                if (!basises1[i].IsAlmostEqualTo(basises2[i], angleTolerance))
                {
                    return (basises1[i], basises2[i]);
                }
            }

            return (null, null);
        }

    }
}
