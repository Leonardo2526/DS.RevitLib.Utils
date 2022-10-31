using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.SystemTree;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class MEPCurvesModel : MEPElementsModel
    {
        private readonly Document _doc;

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
        public MEPCurvesModel WithElbows()
        {
            FamilyInstance familyInstance;

            for (int i = 0; i < MEPCurves.Count - 1; i++)
            {
                familyInstance = FamInstCreator.CreateElbow(MEPCurves[i] as MEPCurve, MEPCurves[i + 1] as MEPCurve);
                AllElements.Insert(i + 1, familyInstance);
            }
            return this;
        }

        /// <summary>
        /// Align ducts by <paramref name="baseMEPCurve"/>.
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <returns>Returns aligned MEPCurves.</returns>
        public MEPCurvesModel RefineDucts(MEPCurve baseMEPCurve)
        {
            if (!baseMEPCurve.IsRectangular())
            {
                return this;
            }

            List<MEPCurve> mEPCurves = MEPCurves.Cast<MEPCurve>().ToList();
            MEPCurve currentBaseCurve = baseMEPCurve;
            for (int i = 0; i < mEPCurves.Count; i++)
            {
                MEPCurve mEPCurve = mEPCurves[i];
                MEPCurveUtils.AlignMEPCurve(currentBaseCurve, mEPCurve);
                currentBaseCurve = mEPCurve;
            }
            return this;
        }

    }
}
