using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.Connection.Strategies
{
    internal abstract class ElementConnectionStrategy : IConnectionStrategy
    {
        protected readonly Document _doc;
        private readonly Element _element1;
        private readonly Element _element2;
        private readonly Element _element3;

        public ElementConnectionStrategy(Document doc)
        {
            _doc = doc;
        }

        public FamilyInstance ConnectionElement { get; protected set; }

        public abstract void Connect();
        public abstract bool IsConnectionAvailable();
    }
}
