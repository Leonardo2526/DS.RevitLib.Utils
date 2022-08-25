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
        private XYZ _rotationPoint;

        public CentralLineSolidRotator(SolidModelExt operationElement, Element targetElement) : 
            base(operationElement, targetElement)
        {
        }

        protected override XYZ GetOperationBaseVector()
        {
            return _operationElement.Line.Direction;
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
            _rotationPoint = _operationElement.Center;

            return Line.CreateBound(_rotationPoint, _rotationPoint + normal);
        }

        public override SolidModelExt Rotate()
        {
            if (_rotationAngle == 0)
            {
                return _operationElement;
            }

            Transform rotateTransform = Transform.CreateRotationAtPoint(_rotationAxis.Direction, _rotationAngle, _rotationPoint);
            _operationElement.Transform(rotateTransform);

            return _operationElement;
        }

        protected override Line GetOperationLine(SolidModelExt operationElement)
        {
            return operationElement.Line;
        }
    }
}
