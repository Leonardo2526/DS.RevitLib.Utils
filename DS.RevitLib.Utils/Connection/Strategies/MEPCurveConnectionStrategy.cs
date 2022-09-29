using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    internal abstract class MEPCurveConnectionStrategy : IConnectionStrategy
    {
        protected readonly MEPCurveGeometryModel _mEPCurve1;
        protected readonly MEPCurveGeometryModel _mEPCurve2;
        protected readonly double _minCurveLength;

        protected MEPCurveConnectionStrategy(
            MEPCurveGeometryModel mEPCurve1, MEPCurveGeometryModel mEPCurve2, double minCurveLength)
        {
            _mEPCurve1 = mEPCurve1;
            _mEPCurve2 = mEPCurve2;
            _minCurveLength = minCurveLength;
        }

        public FamilyInstance ConnectionElement {get; protected set;}

        public abstract bool Connect();
        public abstract bool IsConnectionAvailable();
    }
}
