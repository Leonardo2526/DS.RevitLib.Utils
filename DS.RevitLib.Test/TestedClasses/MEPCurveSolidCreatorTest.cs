using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Solids;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class MEPCurveSolidCreatorTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly TransactionBuilder _transactionBuilder;

        public MEPCurveSolidCreatorTest(Document doc, UIDocument uiDoc)
        {
            Debug.IndentLevel = 1;
            _uiDoc = uiDoc;
            _doc = doc;
            _transactionBuilder = new TransactionBuilder(doc);
        }

        public void Run()
        {
            Reference reference1 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select object1");
            MEPCurve mEPCurve1 = _doc.GetElement(reference1) as MEPCurve;

            Reference reference2 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select object2");
            MEPCurve mEPCurve2 = _doc.GetElement(reference2) as MEPCurve;
            MEPCurve connectedMEPCurve = mEPCurve2;

            var (con1, con2) = ConnectorUtils.GetMainConnectors(mEPCurve1);
            //XYZ point = mEPCurve.GetCenterPoint();
            XYZ point = con1.Origin;

            var creator = new MEPCurveSolidCreator(mEPCurve1, connectedMEPCurve, 100.MMToFeet());
            creator.CreateSolid(point);
            creator.ShowSolid(_doc);
        }
    }
}
