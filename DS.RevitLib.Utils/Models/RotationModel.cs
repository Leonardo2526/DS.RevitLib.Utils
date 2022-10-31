using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.Models
{
    public class RotationModel
    {
        public RotationModel(Line axis, double angle)
        {
            Axis = axis;
            Angle = angle;
        }

        public Line Axis { get; set; }
        public double Angle { get; set; }
    }
}
