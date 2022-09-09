using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.TransactionCommitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class MEPCurvesModel : MEPElementsModel
    {
        protected readonly Document _doc;
        private readonly Committer _committer;
        private readonly string _transactionPrefix;

        public MEPCurvesModel(MEPElementsModel mEPSystemModel, Document doc, Committer committer, 
            string transactionPrefix)
        {
            _doc = doc;
            this._committer = committer;
            AllElements = mEPSystemModel.AllElements;
            MEPCurves = mEPSystemModel.MEPCurves;
            _transactionPrefix = transactionPrefix;
        }


        public MEPElementsModel WithFittings()
        {
            FamInstCreator famInstCreator = new FamInstCreator(_doc, _committer, _transactionPrefix);
            FamilyInstance familyInstance;

            for (int i = 0; i < MEPCurves.Count - 1; i++)
            {
                familyInstance = famInstCreator.
                    CreateFittingByMEPCurves(MEPCurves[i] as MEPCurve, MEPCurves[i + 1] as MEPCurve);
                ErrorMessages += famInstCreator.ErrorMessages;
                AllElements.Insert(i + 1, familyInstance);
            }
            return this;
        }
    }
}
