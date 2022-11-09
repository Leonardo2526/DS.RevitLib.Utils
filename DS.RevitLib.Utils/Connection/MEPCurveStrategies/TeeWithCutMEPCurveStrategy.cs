using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using System;
using System.Collections.Generic;

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

        public override void Connect()
        {
            XYZ cutPoint = _mEPCurve2.Line.Project(_elem1Con.Origin).XYZPoint;
            MEPCurve newMEPCurve = null;

            ElementId newId = _mEPCurve2.MEPCurve.GetType().Name == "Pipe" ?
                PlumbingUtils.BreakCurve(_doc, _mEPCurve2.MEPCurve.Id, cutPoint) :
                MechanicalUtils.BreakCurve(_doc, _mEPCurve2.MEPCurve.Id, cutPoint);
            newMEPCurve = (MEPCurve)_doc.GetElement(newId);

            (Connector con1, Connector con2) = ConnectorUtils.GetMainConnectors(newMEPCurve);
            var (c1, c2) = ConnectorUtils.GetNeighbourConnectors(_mEPCurve2.Connectors, new List<Connector> { con1, con2 });
            var r1 = c1.Owner;
            var r2 = c2.Owner;
            ConnectionElement = _doc.Create.NewTeeFitting(c1, c2, _elem1Con);
            Insulation.Create(_mEPCurve1.MEPCurve, ConnectionElement);
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
