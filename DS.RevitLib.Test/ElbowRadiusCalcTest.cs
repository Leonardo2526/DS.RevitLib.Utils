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
    internal class BestElbowRadiusCalcTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uidoc;

        public BestElbowRadiusCalcTest(Document doc, UIDocument uidoc)
        {
            _doc = doc;
            _uidoc = uidoc;
        }

        public void Run()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var mc1 = _doc.GetElement(reference) as MEPCurve;

            var solidModel = new SolidModel(ElementUtils.GetSolid(mc1));
            MEPCurveModel model = new MEPCurveModel(mc1, solidModel);
            var elbowRadius = new ElbowRadiusCalc(model, new TransactionBuilder(_doc)).GetRadius(90.DegToRad());

            Debug.WriteLine("Elbow radius is " + elbowRadius.FytTomm2());
        }

    }
}
