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
        private readonly Basis _sourceBasis;
        private readonly Basis _operationBasis;

        public TransformBuilder(Basis sourceBasis, Basis operationBasis)
        {
            _sourceBasis = sourceBasis;
            _operationBasis = operationBasis;
        }

        public TransformModel Build()
        {
            var transformModel = new TransformModel();

            transformModel.MoveVector = _operationBasis.Point - _sourceBasis.Point;
            Transform transform = Transform.CreateTranslation(transformModel.MoveVector);

            _sourceBasis.Transform(transform);



            (XYZ basis1, XYZ basis2) = GetNotEqualBasises(_sourceBasis, _operationBasis);
            while(basis1 is not null)
            {
                double angle;
                XYZ axis = basis1.CrossProduct(basis2);
                if (axis.IsZeroLength())
                {
                    angle = 180.DegToRad();
                    axis = XYZUtils.GetPerpendicular(basis1, 
                        new List<XYZ>() { _sourceBasis.X, _sourceBasis.Y, _sourceBasis.Z }).First();
                }
                else
                {
                    angle = basis1.AngleTo(basis2);
                }
                transform = Transform.CreateRotation(axis, angle);
                _sourceBasis.Transform(transform);

                Line axisLine = Line.CreateBound(_sourceBasis.Point, _sourceBasis.Point + axis);
                transformModel.Rotations.Add(new RotationModel(axisLine, angle));
                (basis1, basis2) = GetNotEqualBasises(_sourceBasis, _operationBasis);
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
                if (!basises1[i].IsAlmostEqualTo(basises2[i]))
                {
                    return (basises1[i], basises2[i]);
                }
            }

            return (null, null);
        }     

    }
}
