using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Creator.Builders;
using DS.RevitLib.Utils.TransactionCommitter;
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
        private List<XYZ> _Points = new List<XYZ>();
        private readonly Committer _committer;

        public BuilderByPoints(MEPCurve baseMEPCurve, List<XYZ> points, Committer committer=null, string transactionPrefix = "") : 
            base(baseMEPCurve, transactionPrefix)
        {
            this._Points = points;
            if (committer is null)
            {
                _committer = new BaseCommitter();
            }
            else
            {
                _committer = committer;
            }
        }


        public override MEPCurvesModel BuildMEPCurves()
        {
            MEPCurveCreator mEPCurveCreator = new MEPCurveCreator(BaseMEPCurve, _committer, TransactionPrefix);
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

            return new MEPCurvesModel(MEPSystemModel, Doc, _committer, TransactionPrefix, mEPCurveCreator.ErrorMessages);
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
                    MEPCurveCreator mEPCurveCreator = new MEPCurveCreator(mEPCurve, _committer, TransactionPrefix);
                    mEPCurveCreator.SwapSize(mEPCurve);
                }
            }
        }

    }
}
