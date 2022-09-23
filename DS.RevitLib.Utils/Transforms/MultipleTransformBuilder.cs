using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Transforms
{
    public abstract class MultipleTransformBuilder
    {
        protected List<object> _sourceObjects;
        protected List<object> _targetObjects;

        protected MultipleTransformBuilder()
        { }

        public abstract List<TransformModel> Build(List<object> sourceObjects, List<object> targetObjects, 
            List<XYZ> path, AbstractElementModel startModel);

        protected abstract TransformModel GetModel(object sourceObject, object targetObject);
    }
}
