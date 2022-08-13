using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.MEP.SystemTree.ConnectedBuilders
{
    internal class ConnectedElementsBuilder : AbstractConnectedBuilder<Element>
    {
        private readonly Element _exludedElement;

        public ConnectedElementsBuilder(Element element, Element exludedElement = null) : base(element)
        {
            _exludedElement = exludedElement;
        }

        public List<Element> Build()
        {
            IConnectedBuilder builder = GetBuilder(_element);
            if (_exludedElement is null)
            {
                return builder.Build();
            }

            return builder.Build(_exludedElement);
        }

        private IConnectedBuilder GetBuilder(Element element)
        {
            if (ElementUtils.IsElementMEPCurve(element))
            {
                MEPCurve mEPCurve = element as MEPCurve;
                return new OrderedConnectedBuilder(mEPCurve);
            }

            return new NotOrderedConnectedBuilder(element);

        }
    }
}
