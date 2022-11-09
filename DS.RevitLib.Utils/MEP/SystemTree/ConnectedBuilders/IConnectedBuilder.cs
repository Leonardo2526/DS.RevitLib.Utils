using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.MEP.SystemTree.ConnectedBuilders
{
    public interface IConnectedBuilder
    {
        public List<Element> Build(Element exludedElement);
        public List<Element> Build();

    }
}
