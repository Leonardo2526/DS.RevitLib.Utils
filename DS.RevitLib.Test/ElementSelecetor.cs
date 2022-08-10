using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Test
{
    internal class ElementSelecetor
    {
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public ElementSelecetor(UIDocument uidoc, Document doc, UIApplication uiapp)
        {
            Uidoc = uidoc;
            Doc = doc;
            Uiapp = uiapp;
        }

        public void Select()
        {
            // Find collisions between elements and a selected element
            Reference reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select element that will be checked for intersection with all elements");
            Element element = Doc.GetElement(reference);

            var mEPSystemBuilder = new MEPSystemBuilder(element);
            var system = mEPSystemBuilder.Build();

            var elemsCount = system.Composite.Children.Count;

            HighlightElements(system.ParentElements);
            HighlightElements(system.AllElements);

            TaskDialog.Show("Revit", "There are " + elemsCount.ToString() + " top level parent elements in model.");
        }

        private void HighlightElements(List<Element> elements)
        {
            ICollection<ElementId> ids = new List<ElementId>();
            foreach (var elem in elements)
            {
                ids.Add(elem.Id);
            }

            UIDocument uiDoc = new UIDocument(elements.First().Document);

            uiDoc.Selection.SetElementIds(ids);

            uiDoc.ShowElements(ids);
        }
    }
}
