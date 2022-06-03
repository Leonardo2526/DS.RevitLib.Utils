using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class MEPSystemModel
    {

        #region Properties

        public List<Element> MEPCurves { get; set; } = new List<Element>();
        public List<Element> AllElements { get; set; } = new List<Element>();

        public Connector StartConnector
        {
            get
            {
                var cons1 = ConnectorUtils.GetFreeConnector(AllElements.First());
                return cons1.FirstOrDefault();
            }
        }
        public Connector EndConnector
        {
            get
            {
                var cons1 = ConnectorUtils.GetFreeConnector(AllElements.Last());
                return cons1.FirstOrDefault();
            }
        }

        public string ErrorMessages { get; set; }

        #endregion
    }
}
