using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Creators;
using DS.RevitLib.Utils.Extensions;

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
            if (_rotationAngle == 0)
            {
                return _operationElement;
            }
            _creator.Rotate(_operationElement, _rotationAxis, _rotationAngle);

            return null;
        }

        protected override Line GetOperationLine(Element operationElement)
        {
            return operationElement.GetCenterLine();
        }
    }
}
