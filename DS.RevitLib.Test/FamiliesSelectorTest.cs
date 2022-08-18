using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class FamiliesSelectorTest
    {
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public FamiliesSelectorTest(UIDocument uidoc, Document doc, UIApplication uiapp)
        {
            Uidoc = uidoc;
            Doc = doc;
            Uiapp = uiapp;
        }

        public List<FamilyInstance> Families { get; private set; }
        public MEPCurve MEPCurve { get; private set; }

        public void RunTest()
        {
            // Find collisions between elements and a selected element
            Reference reference = Uidoc.Selection.PickObject(ObjectType.Element, "Select element that will be checked for intersection with all elements");
            Element element = Doc.GetElement(reference);

            var mEPSystemBuilder = new SimpleMEPSystemBuilder(element);
            //var mEPSystemBuilder = new MEPSystemBuilder(element);
            var system = mEPSystemBuilder.Build();

            //var elemsCount = system.Composite.Children.Count;

            var elements = system.GetElements(system.Composite);
            var rootElements = system.GetRootElements(system.Composite);
            var rootFamilies = rootElements.OfType<FamilyInstance>();

            var elemFamilies = rootElements.Where(x => x.Category.Name.Contains("Accessories") || x.Category.Name.Contains("Арматура")).ToList();


            //selection
            var selectedElemFamilies = SelectFilter(rootElements);
            Families = selectedElemFamilies.Cast<FamilyInstance>().ToList();

            ElementUtils.Highlight(elemFamilies);

            //trasfer
            List<XYZ> points = new List<XYZ>()
            {
                new XYZ(0,0,0),
                new XYZ(20,0,0),
            };

            var comp = system.Composite.Root as MEPSystemComponent;
            MEPCurve baseMEPCurve = comp.BaseElement as MEPCurve;
            var builder = new BuilderByPoints(baseMEPCurve, points);
            var model = builder.BuildMEPCurves();
            MEPCurve = model.MEPCurves.First() as MEPCurve;
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

            var range = elements.FindAll(x => elements.IndexOf(x) > minInd && elements.IndexOf(x) < maxInd);

            return range.Where(x => x.Category.Name.Contains("Accessories") || x.Category.Name.Contains("Арматура")).ToList();

        }
    }
}
