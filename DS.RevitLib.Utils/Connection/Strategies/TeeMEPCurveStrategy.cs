﻿using Autodesk.Revit.DB;
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
    internal class TeeMEPCurveStrategy : MEPCurveConnectionStrategy
    {
        private readonly Connector _elem1Con;

        public TeeMEPCurveStrategy(Document doc,
            MEPCurveGeometryModel mEPCurve1, MEPCurveGeometryModel mEPCurve2, double minCurveLength, Connector elem1Con) :
            base(doc, mEPCurve1, mEPCurve2, minCurveLength)
        {
            _elem1Con = elem1Con;
        }

        public override bool Connect()
        {
            var transaction = new TransactionBuilder<FamilyInstance>(_doc, new RollBackCommitter());

            MEPCurve newMEPCurve = null;
            transaction.Build(() =>
            {
                ElementId newId = _mEPCurve2.MEPCurve.GetType().Name == "Pipe" ?
                    PlumbingUtils.BreakCurve(_doc, _mEPCurve2.MEPCurve.Id, _elem1Con.Origin) :
                    MechanicalUtils.BreakCurve(_doc, _mEPCurve2.MEPCurve.Id, _elem1Con.Origin);
                newMEPCurve = (MEPCurve)_doc.GetElement(newId);

            }, "BreakCurve");

            (Connector con1, Connector con2) = ConnectorUtils.GetMainConnectors(newMEPCurve);
            var (c1, c2) = ConnectorUtils.GetNeighbourConnectors(_mEPCurve2.Connectors, new List<Connector> { con1, con2 });
            var r1 = c1.Owner;
            var r2 = c2.Owner;
            transaction.Build(() =>
            {
                ConnectionElement = _doc.Create.NewTeeFitting(c1, c2, _elem1Con);
            }, "InsertElbow");

            return !transaction.ErrorMessages.Any();
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
