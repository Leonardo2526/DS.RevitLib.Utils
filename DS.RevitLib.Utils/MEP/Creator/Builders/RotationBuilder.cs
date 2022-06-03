using Autodesk.Revit.DB;
using DS.MainUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Creator.Builders
{
    internal class RotationBuilder
    {
        /// <summary>
        /// Vector for alignment
        /// </summary>
        public XYZ AlignAxe { get; }
        private readonly MEPCurve _BaseMEPCurve;
        private readonly MEPCurve _MEPCurve;
        private XYZ BaseDir;
        private XYZ CurrentDir;
        private readonly string TransactionPrefix;

        public RotationBuilder(MEPCurve baseMEPCurve, MEPCurve mEPCurve, string transactionPrefix = "")
        {
            this._BaseMEPCurve = baseMEPCurve;
            this._MEPCurve = mEPCurve;
            TransactionPrefix = transactionPrefix;
           

            this.BaseDir = MEPCurveUtils.GetDirection(baseMEPCurve);
            this.CurrentDir = MEPCurveUtils.GetDirection(mEPCurve);

            this.AlignAxe = GetAlignmentVector(baseMEPCurve);
        }

        public MEPCurve Rotate()
        {  
            //Rotation axe
            XYZ rotationAxe = MEPCurveUtils.GetDirection(_MEPCurve);

            List<XYZ> normOrthoVectors = MEPCurveUtils.GetOrthoNormVectors(_MEPCurve);

            //Vector for alignment with rotation
            XYZ vectorToRotateNorm = normOrthoVectors.First();

            double angleRad = vectorToRotateNorm.AngleTo(AlignAxe);
            double angleDeg = angleRad.RadToDeg();

            double prec = 3.0;
            if (Math.Abs(angleDeg - 0) < prec || Math.Abs(angleDeg - 180) < prec ||
                Math.Abs(angleDeg - 90) < prec || Math.Abs(angleDeg - 270) < prec)
            {
                return _MEPCurve;
            }

            int rotDir = GetRotationSide(AlignAxe, vectorToRotateNorm, rotationAxe);

            MEPCurveCreator mEPCurveCreator = new MEPCurveCreator(_MEPCurve, TransactionPrefix);
            return mEPCurveCreator.Rotate(angleRad * rotDir);
        }

        private XYZ GetAlignmentVector(MEPCurve baseMEPCurve)
        {
            XYZ baseCenterPoint = ElementUtils.GetLocationPoint(_MEPCurve);
            Plane plane = Plane.CreateByNormalAndOrigin(CurrentDir, baseCenterPoint);

            List<XYZ> baseNormals = MEPCurveUtils.GetOrthoNormVectors(baseMEPCurve);

            if (XYZUtils.Collinearity(BaseDir, CurrentDir))
            {
                return baseNormals.First();
            }
            else
            {
                XYZ p1 = plane.ProjectOnto(baseCenterPoint);
                XYZ p2 = plane.ProjectOnto(baseCenterPoint + BaseDir);
                return p2 - p1;
            }            
        }

        private int GetRotationSide(XYZ alignAxe,XYZ vectorToRotateNorm,XYZ rotationAxe)
        {
            if (XYZUtils.BasisEqualToOrigin(alignAxe, vectorToRotateNorm, rotationAxe))
            {
                return -1;
            }

            return 1;          
        }

    }
}
