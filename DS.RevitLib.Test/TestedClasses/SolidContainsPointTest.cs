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
            points.ForEach(p => { p.Show(_doc, 0.2); });

            var intersect = IsIntersect(solid1, points);
            Debug.WriteLine(intersect);
        }
        public void RunMultiple()
        {
            MEPCurve mEPCurve1 = _doc.GetElement(new ElementId(709096)) as MEPCurve;
            var mEPSystem1 = new SimpleMEPSystemBuilder(mEPCurve1).Build();

            var freeCon = ConnectorUtils.GetFreeConnector(_doc.GetElement(new ElementId(709929))).FirstOrDefault();
            int count = 0;
            for (int i = 0; i < 100; i++)
            {
                foreach (var elem in mEPSystem1.AllElements)
                {
                    var solid = ElementUtils.GetSolid(elem);
                    if (solid.Contains(freeCon.Origin))
                    {
                        if (elem.Id.IntegerValue == 709096)
                        {
                            count++;
                            Debug.WriteLine($"Step: {count} is valid.");
                            continue;
                        }
                        else
                        {
                            Debug.Fail($"Error i = {i}. Element = {elem.Id}, Solid volume = {solid.Volume}, " +
                                $"Point = {freeCon.Origin}");
                            return;
                        }
                    }
                }

            }

            Debug.WriteLine($"{count} checks complete successfully!");
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
