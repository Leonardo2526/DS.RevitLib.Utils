using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Models;
using System;

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
            if (_elem1Con.Origin.DistanceTo(_elem2Con.Origin) > 0.001)
            {
                var line = _mEPCurve1.MEPCurve.GetCenterLine();
                _mEPCurve1.MEPCurve.Location.Rotate(line, 2 * Math.PI);
            }
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }

    }
}
