using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Transforms;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Transforms
{
    public class BasisTransformModel : TransformModel
    {
        public BasisTransformModel(Basis sourceObject, Basis targetObject) : base(sourceObject, targetObject)
        {
        }

        public XYZ MoveVector { get; set; }
        public List<RotationModel> Rotations { get; set; } = new List<RotationModel>();
    }
}
