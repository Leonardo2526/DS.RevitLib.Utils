using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
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
    internal class CollisionDetectorTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;

        List<BuiltInCategory> _exludedCathegories = new List<BuiltInCategory>()
            {
                //BuiltInCategory.OST_DuctFittingInsulation,
                //BuiltInCategory.OST_DuctInsulations,
                //BuiltInCategory.OST_DuctCurvesInsulation,
                //BuiltInCategory.OST_PipeFittingInsulation,
                //BuiltInCategory.OST_PipeInsulations,
                //BuiltInCategory.OST_PipeCurvesInsulation,
                BuiltInCategory.OST_TelephoneDevices,
                BuiltInCategory.OST_Materials,
                BuiltInCategory.OST_Rooms,
                //BuiltInCategory.OST_PipeCurves
            };

        public CollisionDetectorTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            Run2();
        }

        private void Run1()
        {
            var e1 = new ElementSelector(_uiDoc).Pick();

            BoundingBoxXYZ boxXYZ = e1.get_BoundingBox(null);
            var outline = GetOutline(boxXYZ);            
            var (elements, linkElementsDict) = new ElementsExtractor(_doc, _exludedCathegories, null).GetAll();

            var detector = new ElementCollisionDetectorFactory(_doc, elements, linkElementsDict)
            {
                MinVolume = 0
            };

            var collisions = detector.GetCollisions(e1);

            var ids = collisions.Select(c => c.Item2.Id).ToList();
            _uiDoc.Selection.SetElementIds(ids);

            foreach (var c in collisions)
            {
                Debug.WriteLine(c.Item1.Id + " - " + c.Item2.Id);
            }
            Debug.WriteLine($"Collisions count is: {collisions.Count}");
        }

        private void Run2()
        {
            var e1 = new ElementSelector(_uiDoc).Pick();

            BoundingBoxXYZ boxXYZ = e1.get_BoundingBox(null);
            var outline = GetOutline(boxXYZ);
            var extractor = new ElementsExtractor(_doc, _exludedCathegories, null);
            var intersectionFactory = new BestElementIntersectionFactory(_doc);

            var detector = new BestCollisionDetector(_doc, intersectionFactory, extractor)
            {
                MinVolume = 0
            };

            var excludedElementsIds = new List<ElementId>
            {
                //new ElementId(702936)
            };

            var excludedElements = new List<Element>();
            excludedElementsIds.ForEach(id => excludedElements.Add(_doc.GetElement(id)));

            detector.ExludedElements = excludedElements;
            var collisions = detector.GetCollisions(e1);

            var ids = collisions.Select(c => c.Item2.Id).ToList();
            _uiDoc.Selection.SetElementIds(ids);

            foreach (var c in collisions)
            {
                Debug.WriteLine(c.Item1.Id + " - " + c.Item2.Id);
            }
            Debug.WriteLine($"Collisions count is: {collisions.Count}");
        }

        Outline GetOutline(BoundingBoxXYZ boxXYZ)
        {
            var transform = boxXYZ.Transform;
            var p1 = transform.Inverse.OfPoint(boxXYZ.Min);
            var p2 = transform.Inverse.OfPoint(boxXYZ.Max);
            (XYZ minPoint, XYZ maxPoint) = DS.RevitLib.Utils.XYZUtils.CreateMinMaxPoints(new List<XYZ> { p1, p2 });
            return new Outline(minPoint, maxPoint);
        }
    }
}
