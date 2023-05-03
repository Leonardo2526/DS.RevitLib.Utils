using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class LinesCoplanarityTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uidoc;

        public LinesCoplanarityTest(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
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

            Debug.WriteLine("Lines coplanarity: " + LineUtils.Coplanarity(line1, line2));
        }

    }
}
