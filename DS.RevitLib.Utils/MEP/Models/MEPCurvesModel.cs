using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.TransactionCommitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class MEPCurvesModel : MEPSystemComponent
    {
        protected readonly Document _doc;
        private readonly Committer _committer;
        private readonly string _transactionPrefix;

        public MEPCurvesModel(MEPSystemComponent mEPSystemModel, Document doc, Committer committer, 
            string transactionPrefix)
        {
            _doc = doc;
            this._committer = committer;
            Elements = mEPSystemModel.Elements;
            MEPCurves = mEPSystemModel.MEPCurves;
            _transactionPrefix = transactionPrefix;
        }


        public MEPSystemComponent WithFittings()
        {
            FamInstCreator famInstCreator = new FamInstCreator(_doc, _committer, _transactionPrefix);
            FamilyInstance familyInstance;

            for (int i = 0; i < MEPCurves.Count - 1; i++)
            {
                familyInstance = famInstCreator.
                    CreateFittingByMEPCurves(MEPCurves[i] as MEPCurve, MEPCurves[i + 1] as MEPCurve);
                Elements.Insert(i + 1, familyInstance);
            }
            return this;
        }
    }
}
