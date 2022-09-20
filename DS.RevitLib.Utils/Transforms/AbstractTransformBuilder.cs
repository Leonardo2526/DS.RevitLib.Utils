using DS.ClassLib.VarUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Transforms
{
    public abstract class AbstractTransformBuilder<T,P> : ITransformBuilder
    {
        protected readonly T _sourceObject;
        protected readonly P _targetObject;
        protected T _operationObject;

        protected AbstractTransformBuilder(T sourceObject, P targetObject)
        {
            _sourceObject = sourceObject;
            _targetObject = targetObject;
        }

        public abstract AbstractTransformModel<T, P> Build();
    }
}
