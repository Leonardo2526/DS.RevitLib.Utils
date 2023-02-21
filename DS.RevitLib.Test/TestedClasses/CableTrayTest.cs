using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.MEP;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
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

            _trb.Build(() => _mEPCurve.Split(point), "Break tray");
        }

        private MEPCurve SelectMEPCurve()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            return _doc.GetElement(reference) as MEPCurve;
        }
    }
}
