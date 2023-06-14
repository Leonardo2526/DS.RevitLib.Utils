using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using System;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    internal class SpudMEPCurveStrategy : MEPCurveConnectionStrategy
    {
        private readonly Connector _elem1Con;

        /// <summary>
        /// Initiate object to connect connectors with spud (tap).
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="mEPCurve1"></param>
        /// <param name="mEPCurve2"></param>
        /// <param name="minCurveLength"></param>
        /// <param name="elem1Con"></param>
        public SpudMEPCurveStrategy(Document doc,
            MEPCurveGeometryModel mEPCurve1, MEPCurveGeometryModel mEPCurve2, double minCurveLength, Connector elem1Con) :
            base(doc, mEPCurve1, mEPCurve2, minCurveLength)
        {
            _elem1Con = elem1Con;
        }

        public override void Connect()
        {
            ConnectionElement = _doc.Create.NewTakeoffFitting(_elem1Con, _mEPCurve2.MEPCurve);
            Insulation.Create(_mEPCurve1.MEPCurve, ConnectionElement);
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
