using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    internal class ElbowElementStrategy : ElementConnectionStrategy
    {
        private readonly Connector _con1;
        private readonly Connector _con2;
        private readonly MEPCurve _baseMEPCurve;

        /// <summary>
        /// Initiate object to insert elbow.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="con1"></param>
        /// <param name="con2"></param>
        /// <param name="baseMEPCurve">Element to copy insulation.</param>
        public ElbowElementStrategy(Document doc, Connector con1, Connector con2, MEPCurve baseMEPCurve = null) :
            base(doc)
        {
            _con1 = con1;
            _con2 = con2;
            _baseMEPCurve = baseMEPCurve;
        }

        public override void Connect()
        {
            ConnectionElement = _doc.Create.NewElbowFitting(_con1, _con2);
            if (_baseMEPCurve is not null)
            {
                Insulation.Create(_baseMEPCurve, ConnectionElement);
            }
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
