using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Transforms;

namespace DS.RevitLib.Utils.Elements.Transfer.TransformModels
{
    internal class BasisLineTransformModel : TransformModel
    {
        public BasisLineTransformModel(Basis sourceObject, Line targetObject) :
            base(sourceObject, targetObject)
        {
        }
    }
}
