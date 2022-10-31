using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using System;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    internal class TeeMEPCurveStrategy : MEPCurveConnectionStrategy
    {
        private readonly MEPCurveGeometryModel _mEPCurve3;
        private readonly Connector _curve1Con;

        /// <summary>
        /// Initiate object to connect mEPCurves by insert tee between them.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="mEPCurve1">Child element</param>
        /// <param name="mEPCurve2">Parent element</param>
        /// <param name="mEPCurve3">Parent element</param>
        /// <param name="minCurveLength"></param>
        public TeeMEPCurveStrategy(Document doc,
            MEPCurveGeometryModel mEPCurve1, MEPCurveGeometryModel mEPCurve2, MEPCurveGeometryModel mEPCurve3, Connector curve1Con, double minCurveLength) :
            base(doc, mEPCurve1, mEPCurve2, minCurveLength)
        {
            _mEPCurve3 = mEPCurve3;
            _curve1Con = curve1Con;
        }

        public override void Connect()
        {
            (Connector c2, Connector c3) = ConnectorUtils.GetClosest(_mEPCurve2.MainConnectors, _mEPCurve3.MainConnectors); // parent connectors

            ConnectionElement = _doc.Create.NewTeeFitting(c3, c2, _curve1Con);
            Insulation.Create(_mEPCurve1.MEPCurve, ConnectionElement);
        }

        public override bool IsConnectionAvailable()
        {
            throw new NotImplementedException();
        }
    }
}
