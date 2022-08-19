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
    public class AngleAlignment :  AbstractCreator, IAlignment
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

            var centerLineRotator = new CenterLineRotator(OperationElement, TargetElement, _creator);
            return centerLineRotator.Rotate();
        }

        public Element AlignCenterLines()
        {
            var normalLineRotator = new NormalLineRotator(OperationElement, TargetElement, _creator);
            return normalLineRotator.Rotate();
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
