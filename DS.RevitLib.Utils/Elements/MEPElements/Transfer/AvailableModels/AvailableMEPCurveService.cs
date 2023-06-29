using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Elements.Transfer.AvailableModels
{
    internal class AvailableMEPCurveService : AbstractAvailableCurveService<MEPCurve>
    {
        public AvailableMEPCurveService(List<MEPCurve> targetCurves, double minCurveLength, double minPlacementLength, bool saveElementsOrder = false) :
            base(targetCurves, minCurveLength, minPlacementLength, saveElementsOrder)
        {
        }

        protected override double GetLength(MEPCurve curve)
        {
            return curve.GetCenterLine().ApproximateLength;
        }
    }
}
