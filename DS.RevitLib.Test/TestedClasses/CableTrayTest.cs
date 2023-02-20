using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.MEP;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Test.TestedClasses
{

    internal class CableTrayTest
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private readonly TransactionBuilder _trb;
        private  MEPCurve _mEPCurve;

        public CableTrayTest(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = _uidoc.Document;
            _trb = new TransactionBuilder(_doc);
        }


        public void Break()
        {
            _mEPCurve = SelectMEPCurve();
            XYZ point = _mEPCurve.GetCenterPoint();

            _trb.Build(() => MEPCurveBreaker.Break(_mEPCurve, point, true),"Break tray");
        }

        public static ElementId BreakConduit(Document doc, ElementId conduitId, XYZ breakPoint)
        {
            var conduit = doc.GetElement(conduitId);
            //copy mepCurveToOptimize as newPipe and move to brkPoint
            var location = conduit.Location as LocationCurve;
            var start = location.Curve.GetEndPoint(0);
            var end = location.Curve.GetEndPoint(1);
            var copiedEls = ElementTransformUtils.CopyElement(doc, conduit.Id, breakPoint - start);
            var newId = copiedEls.First();

            //shorten mepCurveToOptimize and newPipe (adjust endpoints)
            AdjustMepCurve(conduit, start, breakPoint, false);
            AdjustMepCurve(doc.GetElement(newId), breakPoint, end, false);

            return newId;
        }

        public static void AdjustMepCurve(Element mepCurve, XYZ p1, XYZ p2, bool disconnect)
        {
            //if (disconnect)
            //    Disconnect(mepCurve);

            var location = mepCurve.Location as LocationCurve;

            location.Curve = Line.CreateBound(p1, p2);
        }


        private MEPCurve SelectMEPCurve()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            return _doc.GetElement(reference) as MEPCurve;
        }
    }
}
