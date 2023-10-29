using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Various;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class CollisionDetectorByTraceTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;

        public CollisionDetectorByTraceTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            Run();
        }

        private void Run()
        {
            MEPCurve baseMEPCurve = new ElementSelector(_uiDoc).Pick() as MEPCurve;
            MEPCurve mEPCurveToExclude = new ElementSelector(_uiDoc).Pick() as MEPCurve;
            var (con1, con2) = mEPCurveToExclude.GetMainConnectors();

            var (elements, linkElementsDict) = new ElementsExtractor(_doc).GetAll();
            //var detector = new CollisionDetectorByTrace(_doc, baseMEPCurve, new TraceSettings(), false, elements, linkElementsDict);
            //detector.ObjectsToExclude = new List<Element>() { mEPCurveToExclude };

            //var collisions = detector.GetCollisions(con1.Origin.ToPoint3d(), con2.Origin.ToPoint3d(), new ClassLib.VarUtils.Basis.Basis3d());
            //Debug.WriteLine($"Collisions count is: {collisions.Count}");
        }
    }
}
