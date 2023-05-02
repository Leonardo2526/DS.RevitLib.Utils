using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class GetJunctionTest
    {
        private readonly Document _doc;
        private readonly TransactionBuilder _trb;
        private readonly UIDocument _uidoc;

        public GetJunctionTest(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
            _trb = new TransactionBuilder(_doc);
            Run();
        }

        private void Run()
        {
            Reference reference1 = _uidoc.Selection.PickObject(ObjectType.Element, "Select mEPCurve1");
            Reference reference2 = _uidoc.Selection.PickObject(ObjectType.Element, "Select mEPCurve2");
            var mc1 = _doc.GetElement(reference1) as MEPCurve;
            var mc2 = _doc.GetElement(reference2) as MEPCurve;
            var junc = MEPCurveUtils.GetJunction(mc1, mc2);
            if(junc != null)
            {
            Debug.WriteLine(junc.Id);
            _trb.Build(() => _uidoc.Selection.SetElementIds(new List<ElementId> { junc.Id }), "show ");
            }
            else
            {
                Debug.WriteLine("Junction is null");
            }

        }
    }
}
