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

        public Connector StartConnector { get; set; }
        public Connector EndConnector { get; set; }

        #endregion
    }
}
