using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.TransactionCommitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    internal class ElbowElementStrategy : ElementConnectionStrategy
    {
        private readonly List<Connector> _cons1;
        private readonly List<Connector> _cons2;
        private readonly MEPCurve _baseMEPCurve;

        /// <summary>
        /// Initiate object to insert elbow.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="cons1"></param>
        /// <param name="cons2"></param>
        /// <param name="baseMEPCurve">Element to copy insulation.</param>
        public ElbowElementStrategy(Document doc, List<Connector> cons1, List<Connector> cons2, MEPCurve baseMEPCurve = null) :
            base(doc)
        {
            _cons1 = cons1;
            _cons2 = cons2;
            _baseMEPCurve = baseMEPCurve;
        }

        public override void Connect()
        {
            (Connector c1, Connector c2) = ConnectorUtils.GetClosest(_cons1, _cons2);

            ConnectionElement = _doc.Create.NewElbowFitting(c1, c2);
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
