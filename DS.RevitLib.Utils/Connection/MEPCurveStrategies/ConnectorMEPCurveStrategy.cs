using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.TransactionCommitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    internal class ConnectorMEPCurveStrategy : MEPCurveConnectionStrategy
    {
        private readonly Connector _elem1Con;
        private readonly Connector _elem2Con;

        /// <summary>
        /// Initiate object to connect two neighbour connectors.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="mEPCurve1"></param>
        /// <param name="mEPCurve2"></param>
        /// <param name="minCurveLength"></param>
        /// <param name="elem1Con"></param>
        /// <param name="elem2Con"></param>
        public ConnectorMEPCurveStrategy(Document doc,
            MEPCurveGeometryModel mEPCurve1, MEPCurveGeometryModel mEPCurve2, double minCurveLength,
            Connector elem1Con, Connector elem2Con) :
            base(doc, mEPCurve1, mEPCurve2, minCurveLength)
        {
            _elem1Con = elem1Con;
            _elem2Con = elem2Con;
        }

        public override void Connect()
        {
            _elem1Con.ConnectTo(_elem2Con);
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
