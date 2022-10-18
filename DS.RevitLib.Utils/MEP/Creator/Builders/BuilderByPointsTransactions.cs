using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Creator.Builders;
using DS.RevitLib.Utils.Points.Models;
using DS.RevitLib.Utils.TransactionCommitter;
using Ivanov.RevitLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class BuilderByPointsTransactions : MEPElementsModelBuilder
    {
        private List<XYZ> _Points = new List<XYZ>();
        private readonly Committer _committer;

        public BuilderByPointsTransactions(MEPCurve baseMEPCurve, List<XYZ> points, Committer committer=null, string transactionPrefix = "") : 
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


        public override MEPCurvesModelTransaction BuildMEPCurves()
        {
            var mEPCurveCreator = new MEPCurveTransactions(BaseMEPCurve, _committer, TransactionPrefix);
            MEPCurve baseMEPCurve = BaseMEPCurve;

            for (int i = 0; i < _Points.Count - 1; i++)
            {
                XYZ p1 = _Points[i];
                XYZ p2 = _Points[i + 1];

                MEPCurve mEPCurve = mEPCurveCreator.CreateMEPCurveByPoints(p1, p2, baseMEPCurve);

                RectangularFixing(baseMEPCurve, mEPCurve);

                baseMEPCurve = mEPCurve;

                MEPElementsModel.AllElements.Add(mEPCurve);
                MEPElementsModel.MEPCurves.Add(mEPCurve);
            }

            ErrorMessages = mEPCurveCreator.ErrorMessages;
            return new MEPCurvesModelTransaction(MEPElementsModel, Doc, _committer, TransactionPrefix);
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
                    var mEPCurveCreator = new MEPCurveTransactions(mEPCurve, _committer, TransactionPrefix);
                    mEPCurveCreator.SwapSize(mEPCurve);
                }
            }
        }

    }
}
