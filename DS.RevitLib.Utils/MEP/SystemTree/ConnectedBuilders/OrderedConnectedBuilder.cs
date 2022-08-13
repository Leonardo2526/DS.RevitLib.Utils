using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.MEP.SystemTree.ConnectedBuilders
{
    internal class OrderedConnectedBuilder : AbstractConnectedBuilder<MEPCurve>, IConnectedBuilder
    {
        public OrderedConnectedBuilder(MEPCurve element) : base(element)
        {
        }

        public List<Element> Build()
        {
            XYZ basePoint = GetBasePoint(_element);
            return MEPCurveUtils.GetOrderedConnected(_element, basePoint);
        }

        public List<Element> Build(Element excludedElement)
        {
            XYZ basePoint = GetBasePoint(_element, excludedElement);
            var connectedElements = ConnectorUtils.GetConnectedElements(_element).
                Where(x => x.Id != excludedElement.Id).ToList();
            return connectedElements.OrderByPoint(basePoint);
        }

        private XYZ GetBasePoint(MEPCurve mEPCurve)
        {
            var notSpuds = MEPCurveUtils.GetNotSpudConnectors(mEPCurve);
            return notSpuds.First().Origin;
        }

        private XYZ GetBasePoint(MEPCurve mEPCurve, Element excludedElement)
        {
            if (excludedElement.IsSpud() && !MEPCurveUtils.IsRoot(mEPCurve, excludedElement))
            {
                var notSpuds = MEPCurveUtils.GetNotSpudConnectors(mEPCurve);
                return notSpuds.First().Origin;
            }

            var (elem1Con, elem2Con) = ConnectorUtils.GetCommonConnectors(mEPCurve, excludedElement);
            return elem1Con.Origin;
        }

    }
}
