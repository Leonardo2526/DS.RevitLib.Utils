using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    internal class ConnectWithMEPCurve : ElementConnectionStrategy
    {
        private readonly Connector _con1;
        private readonly Connector _con2;
        public MEPCurve _baseMEPCurve;
        public TransactionBuilder _trb;

        public ConnectWithMEPCurve(Document doc, Connector con1, Connector con2, MEPCurve baseMEPCurve, TransactionBuilder trb) : base(doc)
        {
            _con1 = con1;
            _con2 = con2;
            _baseMEPCurve = baseMEPCurve;
            _trb = trb;
        }

        public override void Connect()
        {
            var buildr = new BuilderByPoints(_baseMEPCurve, new List<XYZ> { _con1.Origin, _con2.Origin });
            var model = buildr.BuildSystem();

            // connect connectors
            var (mCon1, mCon2) = ConnectorUtils.GetMainConnectors(model.MEPCurves.First());
            (Connector Item1, Connector Item2) conPair1 = (mCon1.Origin - _con1.Origin).IsZeroLength() ? 
                (mCon1, _con1) : (mCon1, _con2);
            (Connector Item1, Connector Item2) conPair2 = (mCon2.Origin - _con1.Origin).IsZeroLength() ? 
                (mCon2, _con1) : (mCon2, _con2);

            conPair1.Item1.ConnectTo(conPair1.Item2);
            conPair2.Item1.ConnectTo(conPair2.Item2);
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
