using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
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
        private readonly Basis _targetBasis;
        private Basis _opertationBasis;

        public TransformBuilder(Basis sourceBasis, Basis targetBasis)
        {
            _opertationBasis = sourceBasis.Clone();
            _targetBasis = targetBasis;
        }

        public TransformModel Build()
        {
            var transformModel = new TransformModel();

            transformModel.MoveVector = _targetBasis.Point - _opertationBasis.Point;
            Transform transform = Transform.CreateTranslation(transformModel.MoveVector);
            _opertationBasis.Transform(transform);
            transformModel.Transforms.Add(transform);

            bool operOrthogonality = _opertationBasis.IsOrthogonal();
            bool targetOrthogonality = _targetBasis.IsOrthogonal();
            var operOrientation = _opertationBasis.GetOrientaion();
            var targetOrientation = _opertationBasis.GetOrientaion();

            int i = 0;
            (XYZ operBasisVector, XYZ targetBasisVector) = GetNotEqualBasises(_opertationBasis, _targetBasis);
            while(operBasisVector is not null && i<4)
            {
                operOrthogonality = _opertationBasis.IsOrthogonal();
                targetOrthogonality = _targetBasis.IsOrthogonal();
                operOrientation = _opertationBasis.GetOrientaion();
                targetOrientation = _opertationBasis.GetOrientaion();

                double angle;
                XYZ axis = operBasisVector.CrossProduct(targetBasisVector);
                if (axis.IsZeroLength())
                {
                    angle = 180.DegToRad();
                    axis = XYZUtils.GetPerpendicular(operBasisVector, 
                        new List<XYZ>() { _opertationBasis.X, _opertationBasis.Y, _opertationBasis.Z }).First();
                }
                else
                {
                    angle = operBasisVector.AngleTo(targetBasisVector);
                }
                transform = Transform.CreateRotationAtPoint(axis, angle, _opertationBasis.Point);
                _opertationBasis.Transform(transform);
                transformModel.Transforms.Add(transform);

                Line axisLine = Line.CreateBound(_opertationBasis.Point, _opertationBasis.Point + axis);
                transformModel.Rotations.Add(new RotationModel(axisLine, angle));
                (operBasisVector, targetBasisVector) = GetNotEqualBasises(_opertationBasis, _targetBasis);
                i++;
            }

            if (i>3)
            {
                throw new InvalidOperationException("i>4");
            }

            return transformModel;
        }

        private (XYZ basis1, XYZ basis2) GetNotEqualBasises(Basis basis1, Basis basis2)
        {
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
                if (!basises1[i].IsAlmostEqualTo(basises2[i], 3.DegToRad()))
                {
                    return (basises1[i], basises2[i]);
                }
            }

            return (null, null);
        }     

    }
}
