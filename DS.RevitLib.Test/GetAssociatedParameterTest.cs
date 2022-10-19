using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.SelectionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils;

namespace DS.RevitLib.Test
{
    internal class GetAssociatedParameterTest
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;

        public GetAssociatedParameterTest(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
        }

        public void Run()
        {
            //ISelectionFilter selFilter = new FamilyInstanceSelectionFilter();
            Reference reference1 = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var elem1 = _doc.GetElement(reference1) as FamilyInstance;
            Reference reference2 = _uidoc.Selection.PickObject(ObjectType.Element, "Select element2");
            var elem2 = _doc.GetElement(reference2) as FamilyInstance;

            var transactionBuilder = new TransactionBuilder<Element>(_doc);
            transactionBuilder.Build(() => ElementParameter.CopySizeParameters(elem1, elem2), "CopySizeParameters");

            //var pD = MEPElementUtils.GetAssociatedParameter(elem1, BuiltInParameter.CONNECTOR_DIAMETER);
            //var pR = MEPElementUtils.GetAssociatedParameter(elem, BuiltInParameter.CONNECTOR_RADIUS);
        }
    }
}
