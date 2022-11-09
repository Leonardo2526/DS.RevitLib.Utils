using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    internal class TeeElementStrategy : ElementConnectionStrategy
    {
        private readonly List<Connector> _cons1;
        private readonly List<Connector> _cons2;
        private readonly List<Connector> _cons3;
        private readonly MEPCurve _baseMEPCurve;

        /// <summary>
        /// Initiate object to insert tee.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="cons1">Child element</param>
        /// <param name="cons2">Parent element</param>
        /// <param name="cons3">Parent element</param>
        /// <param name="baseMEPCurve">Element to copy insulation.</param>
        public TeeElementStrategy(Document doc, List<Connector> cons1, List<Connector> cons2, List<Connector> cons3, MEPCurve baseMEPCurve = null) :
            base(doc)
        {
            _baseMEPCurve = baseMEPCurve;
            _cons1 = cons1;
            _cons2 = cons2;
            _cons3 = cons3;
        }

        public override void Connect()
        {
            (Connector c2, Connector c3) = ConnectorUtils.GetClosest(_cons2, _cons3);
            Connector c1 = ConnectorUtils.GetClosest(c2, _cons1);

            ConnectionElement = _doc.Create.NewTeeFitting(c3, c2, c1);
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
