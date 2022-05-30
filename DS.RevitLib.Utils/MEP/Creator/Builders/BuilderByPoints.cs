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
        public BuilderByPoints(MEPCurve baseMEPCurve, List<XYZ> points, double elbowAngle) : base(baseMEPCurve)
        {
            this._Points = points;
            this._ElbowAngle = elbowAngle;
        }

        private List<XYZ> _Points = new List<XYZ>();
        private double _ElbowAngle;

        public override MEPCurvesModel BuildMEPCurves()
        {
            MEPCurveCreator mEPCurveCreator = new MEPCurveCreator(Doc, BaseMEPCurve);
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

            return new MEPCurvesModel(MEPSystemModel, Doc);
        }

     

        private void RectangularFixing(MEPCurve baseMEPCurve, MEPCurve mEPCurve)
        {
            if (baseMEPCurve is not null && baseMEPCurve.IsRecangular())
                {
                    RotationBuilder rotationBuilder = new RotationBuilder(baseMEPCurve, mEPCurve);
                    rotationBuilder.Rotate();

                //Check if size of MEPCurve should be swapped.
                if (!MEPCurveUtils.EqualOriented(baseMEPCurve, mEPCurve))
                {
                    MEPCurveCreator.SwapSize(mEPCurve);
                }
            }
        }

    }
}
