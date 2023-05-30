using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Test
{
    internal class GetAllElementsTest
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;

        public GetAllElementsTest(Document doc, UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = doc;
        }

        public void Run()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var element1 = _doc.GetElement(reference);

            MEPCurve mEPCurve1 = _doc.GetElement(element1.Id) as MEPCurve;
            var mEPSystem1 = new SimpleMEPSystemBuilder(mEPCurve1).Build();

            Reference reference2 = _uidoc.Selection.PickObject(ObjectType.Element, "Select element2");
            var element2 = _doc.GetElement(reference2);

            var elements = ConnectorUtils.GetAllConnectedElements(element2, _doc, mEPSystem1.Root.Elements);

            _uidoc.Selection.SetElementIds(new List<ElementId> { elements.Last().Id });

        }
    }
}
