using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    internal abstract class AbstractConnectedBuilder<T>
    {
        protected readonly T _element;

        public AbstractConnectedBuilder(T element)
        {
            _element = element;
        }

    }
}
