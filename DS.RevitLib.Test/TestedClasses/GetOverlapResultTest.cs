using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry.Lines;
using DS.RevitLib.Utils.MEP;
using System.Diagnostics;

namespace DS.RevitLib.Test
{
    internal class GetOverlapResultTest
    {
        private readonly Document _doc;
        private readonly TransactionBuilder _trb;
        private readonly UIDocument _uidoc;

        public GetOverlapResultTest(UIDocument uidoc)
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

            var result = LineUtils.GetOverlapResult(line1, line2);
            Debug.WriteLine(result);

           _trb.Build(() => Connect(mc1, mc2, result), "Connect");
        }

        private void Connect(MEPCurve mc1, MEPCurve mc2, LineOverlapResult overlapResult)
        {
            switch (overlapResult)
            {
                case LineOverlapResult.SegementOverlap:
                    break;
                case LineOverlapResult.SegmentPointOverlap:
                    var (parentMC, childMC) = MEPCurveUtils.GetRelation(mc1, mc2, out _);
                    childMC.Connect(parentMC);
                    break;
                case LineOverlapResult.PointOverlap:
                    mc1.Connect(mc2);
                    break;
                case LineOverlapResult.None:
                    break;
                default:
                    break;
            }
        }
    }
}
