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
    internal class TeeWithCutMEPCurveStrategy : MEPCurveConnectionStrategy
    {
        private readonly Connector _elem1Con;

        /// <summary>
        /// Initiate object to connect mEPCurve1 with mEPCurve2 by insert tee into mEPCurve2.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="mEPCurve1">Child element</param>
        /// <param name="mEPCurve2">Parent element</param>
        /// <param name="minCurveLength"></param>
        /// <param name="elem1Con">Child element's connector closest to connect to parent element.</param>
        public TeeWithCutMEPCurveStrategy(Document doc,
            MEPCurveGeometryModel mEPCurve1, MEPCurveGeometryModel mEPCurve2, double minCurveLength, Connector elem1Con) :
            base(doc, mEPCurve1, mEPCurve2, minCurveLength)
        {
            _elem1Con = elem1Con;
        }

        public override bool Connect()
        {
            XYZ cutPoint = _mEPCurve2.Line.Project(_elem1Con.Origin).XYZPoint;

            var transaction = new TransactionBuilder<FamilyInstance>(_doc, new RollBackCommitter());

            MEPCurve newMEPCurve = null;
            transaction.Build(() =>
            {
                ElementId newId = _mEPCurve2.MEPCurve.GetType().Name == "Pipe" ?
                    PlumbingUtils.BreakCurve(_doc, _mEPCurve2.MEPCurve.Id, cutPoint) :
                    MechanicalUtils.BreakCurve(_doc, _mEPCurve2.MEPCurve.Id, cutPoint);
                newMEPCurve = (MEPCurve)_doc.GetElement(newId);

            }, "BreakCurve");

            (Connector con1, Connector con2) = ConnectorUtils.GetMainConnectors(newMEPCurve);
            var (c1, c2) = ConnectorUtils.GetNeighbourConnectors(_mEPCurve2.Connectors, new List<Connector> { con1, con2 });
            var r1 = c1.Owner;
            var r2 = c2.Owner;
            transaction.Build(() =>
            {
                ConnectionElement = _doc.Create.NewTeeFitting(c1, c2, _elem1Con);
                Insulation.Create(_mEPCurve1.MEPCurve, ConnectionElement);
            }, "InsertTee");

            return !transaction.ErrorMessages.Any();
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
