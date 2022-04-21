using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class MEPCurvesModel : MEPSystemModel
    {
        public MEPCurvesModel(MEPSystemModel mEPSystemModel, Document doc)
        {
            Doc = doc;
            AllElements = mEPSystemModel.AllElements;
            MEPCurves = mEPSystemModel.MEPCurves;
        }

        protected readonly Document Doc;

        public MEPSystemModel WithFittings()
        {
            FamInstCreator famInstCreator = new FamInstCreator(Doc);
            FamilyInstance familyInstance;

            for (int i = 0; i < MEPCurves.Count - 1; i++)
            {
                familyInstance = famInstCreator.
                    CreateFittingByMEPCurves(MEPCurves[i] as MEPCurve, MEPCurves[i + 1] as MEPCurve);
                AllElements.Insert(i + 1, familyInstance);
            }

            return this;
        }
    }
}
