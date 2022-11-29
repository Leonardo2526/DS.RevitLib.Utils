using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class SolidContainsPointTest
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;

        public SolidContainsPointTest(Document doc, UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = doc;
        }

        public void Run()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var element1 = _doc.GetElement(reference);

            reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element2 to extract points");
            var element2 = _doc.GetElement(reference);

            Solid solid1 = ElementUtils.GetSolid(element1);
            Solid solid2 = ElementUtils.GetSolid(element2);

            var points = solid2.ExtractPoints();
            points.ForEach(p => { p.Show(_doc, 0.2, new TransactionBuilder(_doc)); });

            var intersect = IsIntersect(solid1, points);
            Debug.WriteLine(intersect);
        }

        private bool IsIntersect(Solid solid, List<XYZ> points)
        {
            foreach (var point in points)
            {
                if (solid.Contains(point))
                { return true; }
            }

            return false;
        }
    }
}
