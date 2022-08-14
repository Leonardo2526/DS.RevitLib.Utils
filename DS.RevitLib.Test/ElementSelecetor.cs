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

            var mEPSystemBuilder = new NewMEPSystemBuilder(element);
            //var mEPSystemBuilder = new MEPSystemBuilder(element);
            var system = mEPSystemBuilder.Build();

            //var elemsCount = system.Composite.Children.Count;

            var elements = system.GetElements(system.Composite);
            var rootElements = system.GetRootElements(system.Composite);
            var rootFamilies = rootElements.OfType<FamilyInstance>();

            var families = rootElements.Where(x => x.Category.Name.Contains("Accessories") || x.Category.Name.Contains("Арматура")).ToList();

            var selectedFamilies = SelectFilter(rootElements);

            HighlightElements(selectedFamilies);
            //HighlightElements(rootElements);
            //HighlightElements(system.ParentElements);
            //HighlightElements(system.AllElements);

            //TaskDialog.Show("Revit", "There are " + elemsCount.ToString() + " top level parent elements in model.");
        }


        private List<Element> SelectFilter(List<Element> elements)
        {
            Reference reference1 = Uidoc.Selection.PickObject(ObjectType.Element, "Select first element.");
            Element element1 = Doc.GetElement(reference1);

            Reference reference2 = Uidoc.Selection.PickObject(ObjectType.Element, "Select second element.");
            Element element2 = Doc.GetElement(reference2);

            var elemsIds = elements.Select(obj => obj.Id).ToList();


            int ind1 = elemsIds.IndexOf(element1.Id);
            int ind2 = elemsIds.IndexOf(element2.Id);

            int minInd = Math.Min(ind1, ind2);
            int maxInd = Math.Max(ind1, ind2);

            var range =  elements.FindAll(x => elements.IndexOf(x) > minInd && elements.IndexOf(x) < maxInd);  

            return range.Where(x => x.Category.Name.Contains("Accessories") || x.Category.Name.Contains("Арматура")).ToList();
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
