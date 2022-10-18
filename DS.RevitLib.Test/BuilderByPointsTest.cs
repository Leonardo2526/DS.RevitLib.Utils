using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP.Creator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class BuilderByPointsTest
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;

        public BuilderByPointsTest(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
        }

        public void Run()
        {
            var points = new List<XYZ>
            {
                new XYZ(0, 0, 0), new XYZ(3, 0, 0),
                new XYZ(3, 3, 0), new XYZ(6, 3, 0),
                new XYZ(6, 6, 0), new XYZ(9, 6, 0),
                new XYZ(9, 9, 0), new XYZ(12, 9, 0),
            };

           Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select MEPCurve");
            MEPCurve mEPCurve = _doc.GetElement(reference) as MEPCurve;

            var transactionBuilder = new TransactionBuilder<MEPCurve>(_doc);

            var builder = new BuilderByPoints(mEPCurve, points);
            //var builder = new BuilderByPointsTransactions(mEPCurve, points);
            //transactionBuilder.Build(() => builder.BuildMEPCurves(), "Create MEPSystem");
            transactionBuilder.Build(() => builder.BuildMEPCurves().WithElbows(), "Create MEPSystem");
        }
    }
}
