using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.Symbols
{
    internal class SymbolModel
    {
        private FamilySymbol familySymbol;
        private Dictionary<Parameter, double> parameters;

        public SymbolModel(FamilySymbol familySymbol, Dictionary<Parameter, double> parameters)
        {
            FamilySymbol = familySymbol;
            Parameters = parameters;
        }

        public FamilySymbol FamilySymbol { get => familySymbol; private set => familySymbol = value; }
        public Dictionary<Parameter, double> Parameters { get => parameters; private set => parameters = value; }
    }
}
