using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.Various;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class MEPSystemGraphFactoryTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;

        public MEPSystemGraphFactoryTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = uidoc.Document;
            Run();
        }

        public void Run()
        {
            var e1 = new ElementSelector(_uiDoc).Pick();

            var facrory = new MEPSystemGraphFactory(_doc)
            {
                TransactionFactory = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside),
                UIDoc = _uiDoc
            };

            var graph = facrory.Create(e1);

        }
    }
}
