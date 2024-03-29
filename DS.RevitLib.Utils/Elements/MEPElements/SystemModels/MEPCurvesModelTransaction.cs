﻿using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.TransactionCommitter;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class MEPCurvesModelTransaction : MEPElementsModel
    {
        protected readonly Document _doc;
        private readonly Committer _committer;
        private readonly string _transactionPrefix;

        public MEPCurvesModelTransaction(MEPElementsModel mEPSystemModel, Document doc, Committer committer,
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
            FamInstTransactions famInstCreator = new FamInstTransactions(_doc, _committer, _transactionPrefix);
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
