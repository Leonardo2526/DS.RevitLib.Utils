using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.TransactionCommitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class AlignMEPCurvesTest
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;

        public AlignMEPCurvesTest(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
        }

        public void Run()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var mc1 = _doc.GetElement(reference) as MEPCurve;
            reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element2");
            var mc2 = _doc.GetElement(reference) as MEPCurve;

            //var transactionBuilder = new TransactionBuilder<Element>(_doc, new RollBackCommitter());
            //MEPCurveUtils.AlignMEPCurve(mc1, mc2, transactionBuilder);
        }
    }
}
