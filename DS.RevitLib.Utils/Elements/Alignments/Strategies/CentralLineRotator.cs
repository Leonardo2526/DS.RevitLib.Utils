using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Elements.Creators;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.GPExtractor;
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
    internal class CentralLineRotator : AlignmentRotator<Element>
    {
        private readonly ElementCreator _creator;

        public CentralLineRotator(Element operationElement, Element targetElement, ElementCreator creator) : 
            base(operationElement, targetElement)
        {
            _operationLine = operationElement.GetCenterLine();
            _creator = creator;
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
            XYZ rotationPoint = ElementUtils.GetLocationPoint(_operationElement);

            return Line.CreateBound(rotationPoint, rotationPoint + normal);
        }

        public override Element Rotate()
        {
            if (XYZUtils.Collinearity(_targetBaseVector, _operationBaseVector))
            {
                return _targetElement;
            }
            _creator.Rotate(_operationElement, _rotationAxis, _rotationAngle);

            return null;
        }

    }
}
