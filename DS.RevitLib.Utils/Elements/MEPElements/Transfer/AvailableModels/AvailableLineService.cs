using System.Collections.Generic;

namespace DS.RevitLib.Utils.Elements.Transfer.AvailableModels
{
    internal class AvailableLineService : AbstractAvailableCurveService<LineModel>
    {
        public AvailableLineService(List<LineModel> targetCurves, double minCurveLength, double minPlacementLength, bool saveElementsOrder = false) :
            base(targetCurves, minCurveLength, minPlacementLength, saveElementsOrder)
        {
        }

        protected override double GetLength(LineModel curve)
        {
            return curve.Line.ApproximateLength;
        }
    }
}
