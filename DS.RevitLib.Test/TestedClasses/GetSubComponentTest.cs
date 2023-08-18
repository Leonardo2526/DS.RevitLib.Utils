using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class GetSubComponentTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private List<ElementId> _subElementIds = new List<ElementId>();

        public GetSubComponentTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = _uiDoc.Document;
            Run();
        }
        public void Run()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select familyInst");
            var topFamInst = _doc.GetElement(reference) as FamilyInstance;
            //_subElementIds = topFamInst.GetSubElements();

            Debug.WriteLine($"SubElements count is {_subElementIds.Count}.");
            _subElementIds.ForEach(id => { Debug.WriteLine(id); });



            if (_subElementIds.Count > 0)
            {
                var elem = _doc.GetElement(_subElementIds.First());
                Debug.WriteLineIf(elem is not null, "First sub component id: " + elem.Id);
            }
        }
    }
}
