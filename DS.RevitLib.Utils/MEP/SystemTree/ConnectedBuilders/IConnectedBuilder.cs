using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.ConnectedBuilders
{
    public interface IConnectedBuilder
    {
        public List<Element> Build(Element exludedElement);
        public List<Element> Build();

    }
}
