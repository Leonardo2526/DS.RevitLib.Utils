using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.Elements.Transfer
{
    internal class TargetPlacementModel
    {

        public TargetPlacementModel(LineModel lineModel, XYZ startPlacementPoint, XYZ entPlacementPoint,
           XYZ startPoint, XYZ endPoint)
        {
            LineModel = lineModel;
            StartPlacementPoint = startPlacementPoint;
            EndPlacementPoint = entPlacementPoint;
            StartPoint = startPoint;
            EndPoint = endPoint;

        }

        public LineModel LineModel { get; }
        public XYZ StartPlacementPoint { get; }
        public XYZ EndPlacementPoint { get; }
        public XYZ StartPoint { get; }
        public XYZ EndPoint { get; }
    }

}
