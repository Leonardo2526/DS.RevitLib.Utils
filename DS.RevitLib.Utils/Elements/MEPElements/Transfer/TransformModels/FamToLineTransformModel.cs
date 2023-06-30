using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Transforms;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Elements.Transfer.TransformModels
{
    internal class FamToLineTransformModel : TransformModel
    {
        public FamToLineTransformModel(SolidModelExt sourceObject, LineModel targetObject) :
            base(sourceObject, targetObject)
        {
        }


        public XYZ MoveVector { get; set; }
        public List<RotationModel> Rotations { get; set; } = new List<RotationModel>();
    }
}
