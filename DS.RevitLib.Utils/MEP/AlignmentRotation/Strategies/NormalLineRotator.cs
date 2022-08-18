using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Elements.Creators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.AlignmentRotation.Strategies
{
    /// <summary>
    /// Around element's normal vector rotation strategy
    /// </summary>
    internal class NormalLineRotator : AlignmentRotator
    {
        public NormalLineRotator(Element operationElement, Element targetElement, ElementCreator creator) : 
            base(operationElement, targetElement, creator)
        {
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
            XYZ rotationPoint = _targetLine.Origin;

            return Line.CreateBound(rotationPoint, rotationPoint + normal);
        }

    }
}
