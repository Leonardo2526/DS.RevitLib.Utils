using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Transforms
{
    public abstract class AbstractTransformBuilder<T,P> : ITransformBuilder<T,P>
    {
        protected T _sourceObject;
        protected P _targetObject;

        protected AbstractTransformBuilder(T sourceObject, P targetObject)
        {
            _sourceObject = sourceObject;
            _targetObject = targetObject;
        }

        public abstract AbstractTransformModel<T, P> Build(T sourceObject, P targetObject);
    }
}
