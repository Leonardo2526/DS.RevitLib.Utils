using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Transforms
{
    public interface ITransformBuilder<T,P>
    {
        public abstract AbstractTransformModel<T, P> Build(T sourceObject, P targetObject);
    }
}
