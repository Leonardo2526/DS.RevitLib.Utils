using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    /// <summary>
    /// Leaf
    /// </summary>
    public class MEPSystemComponent : Component
    {
        public MEPSystemComponent(Element baseElement) : base(baseElement)
        {
        }

        #region Properties

        public List<MEPCurve> MEPCurves { get; set; } = new List<MEPCurve>();
        public List<Element> Elements { get; set; } = new List<Element>();

        public Connector StartConnector
        {
            get
            {
                var cons1 = ConnectorUtils.GetFreeConnector(Elements.First());
                return cons1.FirstOrDefault();
            }
        }
        public Connector EndConnector
        {
            get
            {
                var cons1 = ConnectorUtils.GetFreeConnector(Elements.Last());
                return cons1.FirstOrDefault();
            }
        }


        public Element NodeElem1 { get; set; }
        public Element NodeElem2{ get; set; }

        #endregion

        private Element GetNodeElement()
        {
            return Elements.FirstOrDefault();
        }
    }
}
