using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Transforms
{
    public abstract class AbstractMultipleTransformBuilder<T, P> : ITransformBuilder
    {
        protected List<T> _sourceObjects;
        protected List<P> _targetObjects;

        protected AbstractMultipleTransformBuilder(List<T> sourceObjects, List<P> targetObjects)
        {
            _sourceObjects = sourceObjects;
            _targetObjects = targetObjects;
        }

        public abstract AbstractTransformModel<T, P> Build(T sourceObject, P targetObject);

        public abstract List<AbstractTransformModel<T, P>> Build();
    }
}
