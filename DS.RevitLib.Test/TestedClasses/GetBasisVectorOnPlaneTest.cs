using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.MEP;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Test.TestedClasses
{

    internal class GetBasisVectorOnPlaneTest
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private readonly TransactionBuilder _trb;

        public GetBasisVectorOnPlaneTest(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = _uidoc.Document;
            _trb = new TransactionBuilder(_doc);
        }


        public void Run()
        {
            var mEPCurve1 = SelectMEPCurve();
            var mEPCurve2 = SelectMEPCurve();
            var d1 = MEPCurveUtils.GetDirection(mEPCurve1).Normalize();
            var d2 = MEPCurveUtils.GetDirection(mEPCurve2).Normalize();
            var norm = d1.CrossProduct(d2).Normalize();
            var origin = mEPCurve1.GetCenterPoint();

            var basis = new Basis(d1, d2, norm, origin);
            basis.Round();
            var checkPlane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);

            var basisVectorOnPlane = basis.GetBasisVectorOnPlane(checkPlane, false);
            if (basisVectorOnPlane != null)
            { Debug.WriteLine("\n({0}, {1}, {2})", basisVectorOnPlane.X, basisVectorOnPlane.Y, basisVectorOnPlane.Z); }
            else
            { Debug.WriteLine("\nVector is null"); }
        }

        private MEPCurve SelectMEPCurve()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element");
            return _doc.GetElement(reference) as MEPCurve;
        }
    }
}
