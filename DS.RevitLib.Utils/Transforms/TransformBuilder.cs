using DS.ClassLib.VarUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Transforms
{
    public abstract class TransformBuilder
    {
        protected readonly object _sourceObject;
        protected readonly object _targetObject;

        protected TransformBuilder(object sourceObject, object targetObject)
        {
            _sourceObject = sourceObject;
            _targetObject = targetObject;
        }

        public abstract TransformModel Build();
    }
}
