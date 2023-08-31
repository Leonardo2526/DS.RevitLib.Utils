using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
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
    {        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private readonly TransactionBuilder _trb;

        public AlignMEPCurvesTest(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
            _trb = new TransactionBuilder(_doc);
            Run();
        }

        public void Run()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var mc1 = _doc.GetElement(reference);
            var solid = ElementUtils.GetSolid(mc1);
            //_trb.Build(() => solid.ShowShape(_doc), "show");
            _trb.Build(() => solid.ShowShape(_doc), "show");
            _trb.Build(() => solid.ShowEdges(_doc), "show");

            //var transactionBuilder = new TransactionBuilder<Element>(_doc, new RollBackCommitter());
            //MEPCurveUtils.AlignMEPCurve(mc1, mc2, transactionBuilder);
        }
    }
}
