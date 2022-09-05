using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Alignments.Strategies;
using DS.RevitLib.Utils.Elements.Creators;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Elements.Alignments
{
    public class AngleAlignment :  AbstractCreator, IAlignment<Element>
    {     
        private readonly ElementCreator _creator;

        public AngleAlignment(Element operationElement, Element targetElement)
        {
            OperationElement = operationElement;
            TargetElement = targetElement;
            _creator = new ElementCreator(_committer, _transactionPrefix);
        }

        public Element OperationElement {get; private set;}
        public Element TargetElement { get; private set; }

        public Element AlignNormOrths()
        {
            if (!NeedToAlign(TargetElement))
            {
                return OperationElement;
            }

            var rotator = new NormOrthRotator(OperationElement, TargetElement, _creator);
            return rotator.Rotate();
        }

        public Element AlignCenterLines()
        {
            var rotator = new CentralLineRotator(OperationElement, TargetElement, _creator);
            return rotator.Rotate();
        }

        public Element Align()
        {
            AlignCenterLines();
            AlignNormOrths();
            return OperationElement;
        }

        private bool NeedToAlign(Element element)
        {
            if (element is MEPCurve)
            {
                MEPCurve mEPCurve = element as MEPCurve;
                var profileType = MEPCurveUtils.GetProfileType(mEPCurve);
                if (profileType == ConnectorProfileType.Rectangular)
                {
                    (double width, double heigth) = MEPCurveUtils.GetWidthHeight(mEPCurve);
                    if (Math.Round(width, 3) != Math.Round(heigth, 3))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
