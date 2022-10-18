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

        public MEPCurvesModel(MEPElementsModel mEPSystemModel)
        {
            _doc = mEPSystemModel.AllElements.First().Document;
            AllElements = mEPSystemModel.AllElements;
            MEPCurves = mEPSystemModel.MEPCurves;
        }

        /// <summary>
        /// Add elbows to given MEPSystem.
        /// </summary>
        /// <returns>Returns MEPElementsModel with elbows.</returns>
        public MEPElementsModel WithElbows()
        {
            FamilyInstance familyInstance;

            for (int i = 0; i < MEPCurves.Count - 1; i++)
            {
                familyInstance = FamInstCreator.CreateElbow(MEPCurves[i] as MEPCurve, MEPCurves[i + 1] as MEPCurve);
                AllElements.Insert(i + 1, familyInstance);
            }
            return this;
        }
    }
}
