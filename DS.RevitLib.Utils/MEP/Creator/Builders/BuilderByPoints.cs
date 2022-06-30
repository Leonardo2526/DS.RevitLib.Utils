using Autodesk.Revit.DB;
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
        public BuilderByPoints(MEPCurve baseMEPCurve, List<XYZ> points, string transactionPrefix = "") : 
            base(baseMEPCurve, transactionPrefix)
        {
            this._Points = points;
        }

        private List<XYZ> _Points = new List<XYZ>();

        public override MEPCurvesModel BuildMEPCurves()
        {
            MEPCurveCreator mEPCurveCreator = new MEPCurveCreator(BaseMEPCurve, TransactionPrefix);
            MEPCurve baseMEPCurve = BaseMEPCurve;

            for (int i = 0; i < _Points.Count - 1; i++)
            {
                XYZ p1 = _Points[i];
                XYZ p2 = _Points[i + 1];

                MEPCurve mEPCurve = mEPCurveCreator.CreateMEPCurveByPoints(p1, p2, baseMEPCurve);

                RectangularFixing(baseMEPCurve, mEPCurve);

                baseMEPCurve = mEPCurve;

                MEPSystemModel.AllElements.Add(mEPCurve);
                MEPSystemModel.MEPCurves.Add(mEPCurve);
            }

            return new MEPCurvesModel(MEPSystemModel, Doc, TransactionPrefix, mEPCurveCreator.ErrorMessages);
        }

     

        private void RectangularFixing(MEPCurve baseMEPCurve, MEPCurve mEPCurve)
        {
            if (baseMEPCurve is not null && baseMEPCurve.IsRectangular())
                {
                    RotationBuilder rotationBuilder = new RotationBuilder(baseMEPCurve, mEPCurve, TransactionPrefix);
                    rotationBuilder.Rotate();

                //Check if size of MEPCurve should be swapped.
                if (!MEPCurveUtils.EqualOriented(baseMEPCurve, mEPCurve))
                {
                    MEPCurveCreator mEPCurveCreator = new MEPCurveCreator(mEPCurve, TransactionPrefix);
                    mEPCurveCreator.SwapSize(mEPCurve);
                }
            }
        }

    }
}
