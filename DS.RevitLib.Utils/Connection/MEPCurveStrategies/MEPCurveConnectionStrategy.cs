using Autodesk.Revit.DB;
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
            MEPCurveGeometryModel mEPCurve1, MEPCurveGeometryModel mEPCurve2, double minCurveLength,
            TransactionBuilder<Element> transactionBuilder = null)
        {
            _doc = doc;
            _mEPCurve1 = mEPCurve1;
            _mEPCurve2 = mEPCurve2;
            _minCurveLength = minCurveLength;
            TransactionBuilder = transactionBuilder;
            TransactionBuilder ??= new TransactionBuilder<Element>(doc);
        }

        public FamilyInstance ConnectionElement { get; protected set; }

        public TransactionBuilder<Element> TransactionBuilder { get; protected set; }

        public abstract void Connect();
        public abstract bool IsConnectionAvailable();
    }
}
