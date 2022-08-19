﻿using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Creators;
using DS.RevitLib.Utils.Extensions;

namespace DS.RevitLib.Utils.Elements.Alignments.Strategies
{
    internal abstract class AlignmentRotator
    {
        protected readonly Element _operationElement;
        protected readonly Element _targetElement;
        protected readonly Line _operationLine;
        protected readonly Line _targetLine;
        protected readonly ElementCreator _creator;

        protected Line _rotationAxis;
        protected double _rotationAngle;
        protected XYZ _targetBaseVector;
        protected XYZ _operationBaseVector;

        protected AlignmentRotator(Element operationElement, Element targetElement,
            ElementCreator creator)
        {
            _operationElement = operationElement;
            _targetElement = targetElement;
            _creator = creator;
            _operationLine = operationElement.GetCenterLine();
            _targetLine = targetElement.GetCenterLine();
        }


        protected abstract Line GetRotationAxis();

        protected abstract XYZ GetTargetBaseVector();

        protected abstract XYZ GetOperationBaseVector();

        protected abstract double GetRotationAngle(XYZ targetBaseVector, XYZ operationBaseVector);

        public Element Rotate()
        {
            _targetBaseVector = GetTargetBaseVector();
            _operationBaseVector = GetOperationBaseVector();

            if (XYZUtils.Collinearity(_targetBaseVector, _operationBaseVector))
            {
                return _targetElement;
            }

            _rotationAxis = GetRotationAxis();
            _rotationAngle = GetRotationAngle(_targetBaseVector, _operationBaseVector);

            _creator.Rotate(_operationElement, _rotationAxis, _rotationAngle);

            return null;
        }

    }
}