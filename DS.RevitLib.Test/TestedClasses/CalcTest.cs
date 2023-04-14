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
    internal class CalcTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uidoc;

        public CalcTest(UIDocument uidoc)
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

            var solidModel1 = new SolidModel(ElementUtils.GetSolid(mc1));
            MEPCurveModel parentModel = new MEPCurveModel(mc1, solidModel1);

            var solidModel2 = new SolidModel(ElementUtils.GetSolid(mc2));
            MEPCurveModel childModel = new MEPCurveModel(mc2, solidModel2);

            var tee = new TeeCalc(parentModel, childModel);
            //var elbowRadius = new ElbowRadiusCalc(model, new TransactionBuilder(_doc)).GetRadius(90.DegToRad());

            Debug.WriteLine("Length is " + Math.Round(tee.Length.FytTomm2()));
            Debug.WriteLine("Height is " + Math.Round(tee.Height.FytTomm2()));
        }

    }
}
