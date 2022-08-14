using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.MEP.SystemTree.ConnectedBuilders
{
    internal class NotOrderedConnectedBuilder : AbstractConnectedBuilder<Element>, IConnectedBuilder
    {
        public NotOrderedConnectedBuilder(Element element) : base(element)
        {
        }

        public List<Element> Build()
        {
            return ConnectorUtils.GetConnectedElements(_element);
        }

        public List<Element> Build(Element exludedElement)
        {
            return ConnectorUtils.GetConnectedElements(_element).
                Where(x => x.Id != exludedElement.Id).ToList();
        }
    }
}
