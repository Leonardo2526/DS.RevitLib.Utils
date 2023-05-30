using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using System.Diagnostics;

namespace DS.RevitLib.Test
{
    internal class LinesIntersectionTest
    {
        private readonly Document _doc;
        private readonly TransactionBuilder _trb;
        private readonly UIDocument _uidoc;

        public LinesIntersectionTest(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
            _trb = new TransactionBuilder(_doc);
            Run();
        }

        public void Run()
        {
            Reference reference1 = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            Reference reference2 = _uidoc.Selection.PickObject(ObjectType.Element, "Select element2");
            var mc1 = _doc.GetElement(reference1) as MEPCurve;
            var mc2 = _doc.GetElement(reference2) as MEPCurve;
            var line1 = mc1.GetCurve() as Line;
            var line2 = mc2.GetCurve() as Line;
            var p1 = line1.GetEndParameter(0);
            var p2 = line1.GetEndParameter(1);
            var p3 = line2.GetEndParameter(0);
            var p4 = line2.GetEndParameter(1);

            var point = LineUtils.GetIntersectionPoint(line1, line2, false, false);
            //var point = LineUtils.GetFullIntersection(line1, line2, false);
            Debug.WriteLineIf(point is not null, point);
            Debug.WriteLineIf(point is null, "No intersection");
        }
    }
}
