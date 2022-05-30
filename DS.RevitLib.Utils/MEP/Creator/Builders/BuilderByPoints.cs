using Autodesk.Revit.DB;
using DS.MainUtils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Creator.Builders;
using Ivanov.RevitLib.Utils;
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
            MEPCurve baseMEPCurve = BaseMEPCurve;

            for (int i = 0; i < _Points.Count - 1; i++)
            {
                XYZ p1 = _Points[i];
                XYZ p2 = _Points[i + 1];

                MEPCurve mEPCurve = mEPCurveCreator.CreateMEPCurveByPoints(p1, p2, baseMEPCurve);

                if (CheckRotate(baseMEPCurve, mEPCurve))
                {
                    RotationBuilder rotationBuilder = new RotationBuilder(baseMEPCurve, mEPCurve);
                    rotationBuilder.Rotate();
                }

                if (CheckSwap(baseMEPCurve, mEPCurve))
                {
                    MEPCurveUtils.SwapSize(mEPCurve);
                }

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
        private bool CheckSwap(MEPCurve baseMEPCurve, MEPCurve mEPCurve)
        {
            if (baseMEPCurve is not null)
            {
                if (baseMEPCurve.IsRecangular() &&
                    !MEPCurveUtils.IsEqualSize(baseMEPCurve, mEPCurve))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckRotate(MEPCurve baseMEPCurve, MEPCurve mEPCurve)
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

    }
}
