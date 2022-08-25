using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Alignments.Strategies;
using DS.RevitLib.Utils.Elements.Creators;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Elements.Alignments
{
    public class SolidAngleAlignment :  AbstractCreator, IAlignment<SolidModelExt>
    {     

        public SolidAngleAlignment(SolidModelExt operationSolid, Element targetElement)
        {
            OperationElement = operationSolid;
            TargetElement = targetElement;
        }

        public Element TargetElement { get; private set; }
        public SolidModelExt OperationElement { get; private set; }

        public SolidModelExt AlignNormOrths()
        {
            if (!NeedToAlign(TargetElement))
            {
                return OperationElement;
            }

            var rotator = new NormOrthSolidRotator(OperationElement, TargetElement);
            return rotator.Rotate();
        }

        public SolidModelExt AlignCenterLines()
        {
            var rotator = new CentralLineSolidRotator(OperationElement, TargetElement);
            return rotator.Rotate();
        }

        public SolidModelExt Align()
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
