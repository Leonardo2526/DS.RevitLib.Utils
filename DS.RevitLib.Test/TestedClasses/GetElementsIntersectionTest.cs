using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Various;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class GetElementsIntersectionTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        public GetElementsIntersectionTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            Run();
        }

        private void Run()
        {
            var elem1 = new ElementSelector(_uiDoc).Pick();
            var elem2 = new ElementSelector(_uiDoc).Pick();

            Solid solid1 = ElementUtils.GetSolid(elem1);
            Solid solid2 = ElementUtils.GetSolid(elem2);

            var (elements, linkElementsDict) = new ElementsExtractor(_doc).GetAll();

            var detector = new SolidElementCollisionDetectorFactory(_doc, elements, linkElementsDict)
            {
                MinVolume = 0
            };
            var collisions = detector.GetCollisions(solid1, new List<Element> { elem1});
            Debug.WriteLine($"Collisions count is: {collisions.Count}");

            Solid intersectionSolid =  DS.RevitLib.Utils.Solids.SolidUtils.GetIntersection(solid1, solid2);
            Debug.WriteLineIf(collisions.Count >0 && intersectionSolid is not null, $"Intersection solid volume is: {intersectionSolid?.Volume}");
        }
    }
}
