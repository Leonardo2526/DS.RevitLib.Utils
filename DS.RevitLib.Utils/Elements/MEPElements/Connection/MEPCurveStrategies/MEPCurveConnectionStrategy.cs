﻿using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.MEP.Models;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    internal abstract class MEPCurveConnectionStrategy : IConnectionStrategy
    {
        protected readonly Document _doc;
        protected readonly MEPCurveGeometryModel _mEPCurve1;
        protected readonly MEPCurveGeometryModel _mEPCurve2;
        protected readonly double _minCurveLength;

        protected MEPCurveConnectionStrategy(Document doc,
            MEPCurveGeometryModel mEPCurve1, MEPCurveGeometryModel mEPCurve2, double minCurveLength)
        {
            _doc = doc;
            _mEPCurve1 = mEPCurve1;
            _mEPCurve2 = mEPCurve2;
            _minCurveLength = minCurveLength;
        }

        public FamilyInstance ConnectionElement { get; protected set; }

        public abstract void Connect();
        public abstract bool IsConnectionAvailable();
    }
}
