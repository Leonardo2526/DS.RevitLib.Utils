using Autodesk.Revit.DB;
using DS.MainUtils;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class BuilderByPoints : MEPSystemBuilder
    {
        public BuilderByPoints(MEPCurve baseMEPCurve, List<XYZ> points) : base(baseMEPCurve)
        {
            this._Points = points;
        }

        private List<XYZ> _Points = new List<XYZ>();

        public override MEPCurvesModel BuildMEPCurves()
        {
            MEPCurveCreator mEPCurveCreator = new MEPCurveCreator(Doc, BaseMEPCurve);
            MEPCurve baseMEPCurve = null;
            for (int i = 0; i < _Points.Count - 1; i++)
            {
                XYZ p1 = _Points[i];
                XYZ p2 = _Points[i + 1];

                MEPCurve mEPCurve = mEPCurveCreator.CreateMEPCurveByPoints(p1, p2, baseMEPCurve);

                if (CheckRotate(baseMEPCurve, mEPCurve))
                {
                    Rotate(baseMEPCurve, mEPCurve);
                }

                //if (CheckSwap(baseMEPCurve, mEPCurve))
                //{
                //    MEPCurveUtils.SwapSize(mEPCurve);
                //}

                baseMEPCurve = mEPCurve;

                MEPSystemModel.AllElements.Add(mEPCurve);
                MEPSystemModel.MEPCurves.Add(mEPCurve);
            }

            return new MEPCurvesModel(MEPSystemModel, Doc);
        }

        /// <summary>
        /// Check if size of MEPCurve should be swapped.
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <param name="mEPCurve"></param>
        /// <returns>Return true if size of MEPCurve should be swapped.</returns>
        private static bool CheckSwap(MEPCurve baseMEPCurve, MEPCurve mEPCurve)
        {
            if (baseMEPCurve is not null)
            {
                if (baseMEPCurve.IsRecangular() &&
                    !MEPCurveUtils.IsDirectionEqual(baseMEPCurve, mEPCurve) &&
                    !MEPCurveUtils.IsEqualSize(baseMEPCurve, mEPCurve))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CheckRotate(MEPCurve baseMEPCurve, MEPCurve mEPCurve)
        {
            if (baseMEPCurve is not null)
            {
                if (baseMEPCurve.IsRecangular())
                {
                    return true;
                }
            }

            return false;
        }

        private void Rotate(MEPCurve baseMEPCurve, MEPCurve mEPCurve)
        {
            XYZ baseDir = MEPCurveUtils.GetDirection(baseMEPCurve);
            XYZ dir = MEPCurveUtils.GetDirection(mEPCurve);

            Plane plane = null;
            if (baseDir.IsAlmostEqualTo(dir) || baseDir.Negate().IsAlmostEqualTo(dir))
            {
                plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, ElementUtils.GetLocationPoint(mEPCurve));
            }
            else
            {
                plane = MEPCurveUtils.GetPlane(mEPCurve, baseMEPCurve);
            }

            List<XYZ> normOrthoVectors = MEPCurveUtils.GetOrthoNormVectors(mEPCurve);
            List<double> angles = GetAngles(baseDir, normOrthoVectors);


            XYZ normVector = normOrthoVectors.First();
           
            double angleRad = normVector.AngleTo(plane.Normal);
            double angleDeg = angleRad.RadToDeg();
            double rotAngle = 0;
            if (angleDeg <= 90)
            {
                rotAngle = 90 - angleDeg;
            }
            else
            {
                rotAngle = angleDeg - 90;
            }

            double prec = 1.0;           
            if(Math.Abs(rotAngle - 0) < prec || Math.Abs(rotAngle - 180) < prec ||
                Math.Abs(rotAngle - 90) < prec || Math.Abs(rotAngle - 270) < prec)
            {
                return;
            }
                mEPCurve = RotateMEPCurve(mEPCurve, rotAngle.DegToRad()); 
        }

        private MEPCurve RotateMEPCurve(MEPCurve mEPCurve, double angleRad)
        {

            using (Transaction transNew = new Transaction(Doc, "CreateMEPCurveByPoints"))
            {
                try
                {
                    transNew.Start();

                    var locCurve = mEPCurve.Location as LocationCurve;
                    var line = locCurve.Curve as Line;

                    mEPCurve.Location.Rotate(line, angleRad);
                }
                catch (Exception e)

                { }
                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return mEPCurve;
        }

        private List<double> GetAngles(XYZ dir, List<XYZ> normOrthoVectors)
        {
            List<double> angles = new List<double>();
            foreach (var v in normOrthoVectors)
            {
                double angleRad = dir.AngleTo(v);
                double angleDeg = angleRad.RadToDeg();
                angles.Add(angleDeg);
            }

            return angles;
        }

        private bool IsVectorsEqual(List<XYZ> normOrthoVectors, XYZ baseVector)
        {
            foreach (var vector in normOrthoVectors)
            {
                if (baseVector.IsAlmostEqualTo(vector) || baseVector.Negate().IsAlmostEqualTo(vector))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
