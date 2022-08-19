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
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Elements.Alignments.Strategies
{
    /// <summary>
    /// Around element's center line norm orth rotation strategy
    /// </summary>
    internal class NormOrthRotator : AlignmentRotator
    {
        public NormOrthRotator(Element operationElement, Element targetElement, ElementCreator creator) :
            base(operationElement, targetElement, creator)
        {

        }

        protected override Line GetRotationAxis()
        {
            return _operationLine;
        }

        protected override double GetRotationAngle(XYZ targetBaseVector, XYZ operationBaseVector)
        {
            double angleRad = targetBaseVector.AngleTo(operationBaseVector);
            double angleDeg = angleRad.RadToDeg();
            int rotDir = GetRotationSide(_targetBaseVector, _operationBaseVector, _rotationAxis.Direction);

            return angleRad * rotDir;
        }

        protected override XYZ GetTargetBaseVector()
        {
            List<XYZ> normOrths = ElementUtils.GetOrthoNormVectors(_targetElement);
            return ElementUtils.GetMaxSizeOrth(_targetElement, normOrths);
        }

        protected override XYZ GetOperationBaseVector()
        {
            List<XYZ> normOrths = ElementUtils.GetOrthoNormVectors(_operationElement);
            return ElementUtils.GetMaxSizeOrth(_operationElement, normOrths);
        }

        private XYZ GetAlignmentVector(MEPCurve baseMEPCurve)
        {
            XYZ baseCenterPoint = ElementUtils.GetLocationPoint(baseMEPCurve);
            Plane plane = Plane.CreateByNormalAndOrigin(_operationLine.Direction, baseCenterPoint);

            List<XYZ> baseNormals = ElementUtils.GetOrthoNormVectors(baseMEPCurve);

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
