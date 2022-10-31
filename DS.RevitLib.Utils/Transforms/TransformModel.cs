using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Transforms
{
    public abstract class TransformModel
    {
        protected TransformModel(object sourceObject, object targetObject)
        {
            SourceObject = sourceObject;
            TargetObject = targetObject;
        }

        public object SourceObject { get; }
        public object TargetObject { get; }
        public List<Transform> Transforms { get; set; } = new List<Transform>();
    }
}
