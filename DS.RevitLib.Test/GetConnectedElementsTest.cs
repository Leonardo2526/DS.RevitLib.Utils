using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.FamilyInstances;
using DS.RevitLib.Utils.SelectionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Document = Autodesk.Revit.DB.Document;

namespace DS.RevitLib.Test
{
    internal class GetConnectedElementsTest
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;

        public GetConnectedElementsTest(UIDocument uidoc, Document doc)
        {
            _uidoc = uidoc;
            _doc = doc;
        }

        public void Run()
        {
            ISelectionFilter selFilter = new FamilyInstanceSelectionFilter();
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Select element");
            var familyInstance = _doc.GetElement(reference) as FamilyInstance;
            var (parents, child) =  familyInstance.GetConnectedElements();

            _uidoc.Selection.SetElementIds(parents.Select(obj => obj.Id).ToList());
            _uidoc.RefreshActiveView();
            _uidoc.Selection.SetElementIds(new List<ElementId> { child.Id});
        }
    }
}
