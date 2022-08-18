using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Elements.Creators;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.TransactionCommitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.AlignmentRotation.Strategies
{
    /// <summary>
    /// Around element's center line rotation strategy
    /// </summary>
    internal class CenterLineRotator : AlignmentRotator
    {
        public CenterLineRotator(Element operationElement, Element targetElement, ElementCreator creator) : 
            base(operationElement, targetElement, creator)
        {

        }

        protected override Line GetRotationAxis()
        {
            return _operationLine;
        }

        protected override double GetRotationAngle(XYZ targetBaseVector, XYZ operationBaseVector)
        {
            double angleRad = _targetBaseVector.AngleTo(_operationBaseVector);
            double angleDeg = angleRad.RadToDeg();

            double prec = 3.0;
            if (Math.Abs(angleDeg - 0) < prec || Math.Abs(angleDeg - 180) < prec ||
                Math.Abs(angleDeg - 90) < prec || Math.Abs(angleDeg - 270) < prec)
            {
                return 0;
            }

            int rotDir = GetRotationSide(_targetBaseVector, _operationBaseVector, _rotationAxis.Direction);

            return angleRad * rotDir;
        }

        protected override XYZ GetTargetBaseVector()
        {
            if (_targetElement is MEPCurve)
            {
                return GetAlignmentVector(_targetElement as MEPCurve);
            }

            return null;
        }

        protected override XYZ GetOperationBaseVector()
        {
            if (_operationElement is MEPCurve)
            {
                List<XYZ> normOrthoVectors = MEPCurveUtils.GetOrthoNormVectors(_operationElement as MEPCurve);
                return normOrthoVectors.First();
            }

            return null;
        }       

        private XYZ GetAlignmentVector(MEPCurve baseMEPCurve)
        {
            XYZ baseCenterPoint = ElementUtils.GetLocationPoint(baseMEPCurve);
            Plane plane = Plane.CreateByNormalAndOrigin(_operationLine.Direction, baseCenterPoint);

            List<XYZ> baseNormals = MEPCurveUtils.GetOrthoNormVectors(baseMEPCurve);

            if (XYZUtils.Collinearity(_targetLine.Direction, _operationLine.Direction))
            {
                return baseNormals.First();
            }
            else
            {
                XYZ p1 = plane.ProjectOnto(baseCenterPoint);
                XYZ p2 = plane.ProjectOnto(baseCenterPoint + _targetLine.Direction);
                return p2 - p1;
            }
        }

        private int GetRotationSide(XYZ alignAxe, XYZ vectorToRotateNorm, XYZ rotationAxe)
        {
            if (XYZUtils.BasisEqualToOrigin(alignAxe, vectorToRotateNorm, rotationAxe))
            {
                return -1;
            }

            return 1;
        }
    }
}
