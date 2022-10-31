using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    public class MEPElementsModel
    {

        #region Properties

        public List<MEPCurve> MEPCurves { get; set; } = new List<MEPCurve>();
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
