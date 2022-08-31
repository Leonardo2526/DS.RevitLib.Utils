using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.GPExtractor;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Elements.Alignments.Strategies
{
    internal class NormOrthSolidRotator : AlignmentRotator<SolidModelExt>
    {
        public NormOrthSolidRotator(SolidModelExt operationElement, Element targetElement) :
            base(operationElement, targetElement)
        { }

        protected override XYZ GetOperationBaseVector()
        {
            List<XYZ> normOrths = DS.RevitLib.Utils.Solids.SolidUtils.
                GetOrthoNormVectors(_operationElement.Solid, _operationElement.CentralLine);
            return GetMaxSizeOrth(normOrths);
        }

        private XYZ GetMaxSizeOrth(List<XYZ> orths)
        {
            XYZ maxVector = null;
            double maxSize = 0;
            foreach (var orth in orths)
            {
                double size = _operationElement.GetSizeByVector(orth);
                if (size > maxSize)
                {
                    maxSize = size;
                    maxVector = orth;
                }
            }

            return maxVector;
        }

        protected override double GetRotationAngle(XYZ targetBaseVector, XYZ operationBaseVector)
        {
            double angleRad = targetBaseVector.AngleTo(operationBaseVector);
            double angleDeg = angleRad.RadToDeg();
            int rotDir = GetRotationSide(_targetBaseVector, _operationBaseVector, _rotationAxis.Direction);

            return angleRad * rotDir;
        }

        protected override Line GetRotationAxis()
        {
            return _operationLine;
        }

        protected override XYZ GetTargetBaseVector()
        {
            List<XYZ> normOrths = ElementUtils.GetOrthoNormVectors(_targetElement);
            return ElementUtils.GetMaxSizeOrth(_targetElement, normOrths);
        }

        public override SolidModelExt Rotate()
        {
            if (_rotationAngle == 0)
            {
                return _operationElement;
            }

            Transform rotateTransform = Transform.
                CreateRotationAtPoint(_rotationAxis.Direction, _rotationAngle, _operationElement.CentralPoint);
            _operationElement.Transform(rotateTransform);

            return _operationElement;
        }

        private int GetRotationSide(XYZ alignAxe, XYZ vectorToRotateNorm, XYZ rotationAxe)
        {
            if (XYZUtils.BasisEqualToOrigin(alignAxe, vectorToRotateNorm, rotationAxe))
            {
                return -1;
            }

            return 1;
        }

        protected override Line GetOperationLine(SolidModelExt operationElement)
        {
            return operationElement.CentralLine;
        }
    }
}
