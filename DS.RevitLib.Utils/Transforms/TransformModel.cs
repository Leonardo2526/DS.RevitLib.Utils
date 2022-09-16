using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Models
{
    public class TransformModel
    {
        public XYZ MoveVector { get; set; }
        public List<RotationModel> Rotations { get; set; } = new List<RotationModel>();

        public List<Transform> Transforms { get; set; } = new List<Transform>();
    }
}
