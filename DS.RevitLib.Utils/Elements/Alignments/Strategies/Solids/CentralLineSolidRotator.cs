using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Elements.Creators;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.GPExtractor;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Elements.Alignments.Strategies
{
    /// <summary>
    /// Around element's normal orth vector central line rotation strategy
    /// </summary>
    internal class CentralLineSolidRotator : AlignmentRotator<SolidModelExt>
    {
        public CentralLineSolidRotator(SolidModelExt operationElement, Element targetElement) : base(operationElement, targetElement)
        {
            _operationLine = operationElement.Line;
        }

        protected override XYZ GetOperationBaseVector()
        {
            return _operationLine.Direction;
        }
        protected override XYZ GetTargetBaseVector()
        {
            return _targetLine.Direction;
        }

        protected override double GetRotationAngle(XYZ targetBaseVector, XYZ operationBaseVector)
        {
            return _targetBaseVector.AngleTo(_operationBaseVector);
        }

        protected override Line GetRotationAxis()
        {
            XYZ normal = _operationLine.Direction.CrossProduct(_targetLine.Direction);
            XYZ rotationPoint = _operationElement.Center;

            return Line.CreateBound(rotationPoint, rotationPoint + normal);
        }

        public override SolidModelExt Rotate()
        {
            if (XYZUtils.Collinearity(_targetBaseVector, _operationBaseVector))
            {
                return _operationElement;
            }

            Transform rotateTransform = Transform.CreateRotation(_rotationAxis.Direction, _rotationAngle);
            _operationElement.Transform(rotateTransform);

            return _operationElement;
        }

    }
}
