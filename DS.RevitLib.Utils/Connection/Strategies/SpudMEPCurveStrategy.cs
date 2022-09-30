using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using DS.RevitLib.Utils.Extensions;
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
    internal class SpudMEPCurveStrategy : MEPCurveConnectionStrategy
    {
        private readonly Connector _elem1Con;

        public SpudMEPCurveStrategy(Document doc,
            MEPCurveGeometryModel mEPCurve1, MEPCurveGeometryModel mEPCurve2, double minCurveLength, Connector elem1Con) :
            base(doc, mEPCurve1, mEPCurve2, minCurveLength)
        {
            _elem1Con = elem1Con;
        }

        public override bool Connect()
        {
            var transaction = new TransactionBuilder<FamilyInstance>(_doc, new RollBackCommitter());
            transaction.Build(() =>
            {
                ConnectionElement = _doc.Create.NewTakeoffFitting(_elem1Con, _mEPCurve2.MEPCurve);
                Insulation.Create(_mEPCurve1.MEPCurve, ConnectionElement);
            }, "InsertSpud");

            return !transaction.ErrorMessages.Any();
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
