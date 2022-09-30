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
    internal class ElbowMEPCurveStrategy : MEPCurveConnectionStrategy
    {
        private readonly Connector _elem1Con;
        private readonly Connector _elem2Con;

        public ElbowMEPCurveStrategy(Document doc, 
            MEPCurveGeometryModel mEPCurve1, MEPCurveGeometryModel mEPCurve2, 
            double minCurveLength, 
            Connector elem1Con, Connector elem2Con) : 
            base(doc, mEPCurve1, mEPCurve2, minCurveLength)
        {
            _elem1Con = elem1Con;
            _elem2Con = elem2Con;
        }

        public override bool Connect()
        {
            var transaction = new TransactionBuilder<FamilyInstance>(_doc, new RollBackCommitter());
            transaction.Build(() =>
            {              
                ConnectionElement = _doc.Create.NewElbowFitting(_elem1Con, _elem2Con);
                Insulation.Create(_mEPCurve1.MEPCurve, ConnectionElement);
            }, "InsertElbow");

            return !transaction.ErrorMessages.Any();
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
