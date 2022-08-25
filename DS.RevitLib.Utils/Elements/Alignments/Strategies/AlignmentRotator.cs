using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Creators;
using DS.RevitLib.Utils.Extensions;

namespace DS.RevitLib.Utils.Elements.Alignments.Strategies
{
    internal abstract class AlignmentRotator<T>
    {
        protected readonly T _operationElement;
        protected readonly Element _targetElement;
        protected Line _operationLine;
        protected readonly Line _targetLine;

        protected Line _rotationAxis;
        protected double _rotationAngle;
        protected XYZ _targetBaseVector;
        protected XYZ _operationBaseVector;

        protected AlignmentRotator(T operationElement, Element targetElement)
        {
            _operationElement = operationElement;
            _targetElement = targetElement;
            _targetLine = targetElement.GetCenterLine();
            _targetBaseVector = GetTargetBaseVector();
            _operationBaseVector = GetOperationBaseVector();
            _rotationAxis = GetRotationAxis();
            _rotationAngle = GetRotationAngle(_targetBaseVector, _operationBaseVector);
        }


        protected abstract Line GetRotationAxis();

        protected abstract XYZ GetTargetBaseVector();

        protected abstract XYZ GetOperationBaseVector();

        protected abstract double GetRotationAngle(XYZ targetBaseVector, XYZ operationBaseVector);

        public abstract T Rotate();       

    }
}
