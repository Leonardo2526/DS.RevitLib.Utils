using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public FamilyInstance ConnectionElement {get; protected set;}

        public abstract bool Connect();
        public abstract bool IsConnectionAvailable();
    }
}
