using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class MEPSystemBuilderTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uidoc;

        public MEPSystemBuilderTest(Document doc, UIDocument uidoc)
        {
            _doc = doc;
            _uidoc = uidoc;
        }

        public void Run()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var mc1 = _doc.GetElement(reference) as MEPCurve;

            var mEPSystemBuilder = new SimpleMEPSystemBuilder(mc1);
            var sourceMEPModel = mEPSystemBuilder.Build();

            var ids = sourceMEPModel.AllElements.Select(obj => obj.Id).ToList();
            //var ids = sourceMEPModel.Root.Elements.Select(obj => obj.Id).ToList();
            //var ids = sourceMEPModel.Childs.SelectMany(obj => obj.Elements).Select(obj => obj.Id).ToList();
            //var ids = sourceMEPModel.Parents.SelectMany(obj => obj.Elements).Select(obj => obj.Id).ToList();



            _uidoc.Selection.SetElementIds(ids);
            Debug.WriteLine("Selected elements count: " + ids.Count);

            var distincted = ids.Distinct().ToList();

            int duplicate = ids.Count - distincted.Count;
            Debug.WriteLineIf(duplicate> 0, "Duplicates occured: " + duplicate);
        }

        public void ComponentTest()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select base element");
            var baseElement = _doc.GetElement(reference);

            var mEPSystemBuilder = new SimpleMEPSystemBuilder(baseElement);
            var sourceMEPModel = mEPSystemBuilder.Build();

            Reference reference1 = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var element1 = _doc.GetElement(reference1);

            Reference reference2 = _uidoc.Selection.PickObject(ObjectType.Element, "Select element2");
            var element2 = _doc.GetElement(reference2);

            var rootElem = sourceMEPModel.GetRootElements(element1, element2);
            //var rootElem = sourceMEPModel.FindRootElem(element1);

            _uidoc.Selection.SetElementIds(rootElem.Select(obj => obj.Id).ToList());
        }

    }
}
