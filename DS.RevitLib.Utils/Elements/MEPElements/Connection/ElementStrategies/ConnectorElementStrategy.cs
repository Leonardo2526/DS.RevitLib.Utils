using Autodesk.Revit.DB;
using System;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    internal class ConnectorElementStrategy : ElementConnectionStrategy
    {
        private readonly Connector _con1;
        private readonly Connector _con2;

        public ConnectorElementStrategy(Document doc, Connector con1, Connector con2) : base(doc)
        {
            _con1 = con1;
            _con2 = con2;
        }

        public override void Connect()
        {
            _con1.ConnectTo(_con2);
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
