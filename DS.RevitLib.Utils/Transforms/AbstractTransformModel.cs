using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Transforms
{
    public abstract class AbstractTransformModel<T, P>
    {
        protected AbstractTransformModel(T sourceObject, P targetObject)
        {
            SourceObject = sourceObject;
            TargetObject = targetObject;
        }
        
        public T SourceObject { get; }
        public P TargetObject { get; }
        public List<Transform> Transforms { get; set; } = new List<Transform>();
    }
}
