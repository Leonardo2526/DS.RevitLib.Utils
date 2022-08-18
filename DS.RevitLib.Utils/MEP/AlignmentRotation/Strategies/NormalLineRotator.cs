using Autodesk.Revit.DB;
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
            throw new NotImplementedException();
        }

        protected override double GetRotationAngle(XYZ targetBaseVector, XYZ operationBaseVector)
        {
            throw new NotImplementedException();
        }

        protected override Line GetRotationAxis()
        {
            throw new NotImplementedException();
        }

        protected override XYZ GetTargetBaseVector()
        {
            throw new NotImplementedException();
        }
    }
}
