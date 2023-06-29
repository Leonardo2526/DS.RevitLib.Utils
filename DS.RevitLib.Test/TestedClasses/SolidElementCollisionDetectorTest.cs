using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Collisions.Detectors;

using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Elements;
using iUtils.SelctionFilters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.Collisions.Models;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class SolidElementCollisionDetectorTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private SolidElementCollisionDetectorFactory _factory;

        public SolidElementCollisionDetectorTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
        }

        public void Run()
        {
            Reference reference1 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            Element element1 = _doc.GetElement(reference1);
            Solid solid1 = ElementUtils.GetSolid(element1);
           
            //Reference reference2 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            //Element element2 = _doc.GetElement(reference2);
            //Solid solid2 = ElementUtils.GetSolid(element2);
            //var checkObjects1 = new List<Solid>() { solid1, solid2 };

            var extractor = new ElementsExtractor(_doc);
            var (docElements, linkElementsDict) = extractor.GetAll();

            var checkObjects2 = new List<Element>();
            checkObjects2.AddRange(docElements);
            if (linkElementsDict is not null && linkElementsDict.Any())
            {
                List<Element> linkElements = linkElementsDict.SelectMany(obj => obj.Value).ToList();
                checkObjects2.AddRange(linkElements);
            }

            var checkObjects2ToExclude = new List<Element>() { element1 };

            var detector = new SolidElementCollisionDetector(_doc, checkObjects2);
            var collisions = detector.GetCollisions(solid1, checkObjects2ToExclude).Cast<SolidElementCollision>().ToList();

            var collisionElemIds = collisions.Select(obj => obj.Object2.Id).ToList();
            collisions.ForEach(collision => _uiDoc.Selection.SetElementIds(collisionElemIds));
            //_uiDoc.RefreshActiveView();

            if (linkElementsDict is not null && linkElementsDict.Any())
            {
                foreach (var item in linkElementsDict)
                {
                    var linkDetector = new SolidElementCollisionDetector(item.Key, item.Value);
                    //linkDetector.ShowLinkSolids();
                    //_uiDoc.RefreshActiveView();
                    var linkCollisions = linkDetector.GetCollisions(solid1, checkObjects2ToExclude).Cast<SolidElementCollision>().ToList();
                    collisions.AddRange(linkCollisions);
                }
            }

            collisionElemIds = collisions.Select(obj => obj.Object2.Id).ToList();
            if (linkElementsDict is not null && linkElementsDict.Any())
            {
                collisions.ForEach(collision => SelectElementInLink(collision.Object2, linkElementsDict.Keys.ToList()));
            }

            Debug.WriteLine($"Collisions count is {collisions.Count}");
        }

        public void RunWithFactory()
        {
            Reference reference1 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            Element element1 = _doc.GetElement(reference1);
            Solid solid1 = ElementUtils.GetSolid(element1);

            var checkObjects2ToExclude = new List<Element>() { element1 };

            var (docElements, linkElementsDict) = new ElementsExtractor(_doc).GetAll();
            _factory = new SolidElementCollisionDetectorFactory(_doc, docElements, linkElementsDict);
            var collisions = _factory.GetCollisions(solid1, checkObjects2ToExclude).Cast<SolidElementCollision>().ToList();

            //show collisions
            //var collisionElemIds = collisions.Select(obj => obj.Object2.Id).ToList();
            //collisions.ForEach(collision => SelectElementInLink(collision.Object2, factory._linkElementsDict.Keys.ToList()));
            Debug.WriteLine($"\nCollisions count is {collisions.Count}");
            Debug.WriteLine($"Collision objects ids:");
            collisions.ForEach(obj => Debug.WriteLine($"{obj.Object2.Id}"));
        }

        public void RepeatRun()
        {
            Reference reference1 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element2");
            Element element1 = _doc.GetElement(reference1);
            Solid solid1 = ElementUtils.GetSolid(element1);

            var checkObjects2ToExclude = new List<Element>() { element1 };
            var collisions = _factory.GetCollisions(solid1, checkObjects2ToExclude).Cast<SolidElementCollision>().ToList();

            //show collisions
            //var collisionElemIds = collisions.Select(obj => obj.Object2.Id).ToList();
            //collisions.ForEach(collision => SelectElementInLink(collision.Object2, factory._linkElementsDict.Keys.ToList()));
            Debug.WriteLine($"\nCollisions count is {collisions.Count}");
            Debug.WriteLine($"Collision objects ids:");
            collisions.ForEach(obj => Debug.WriteLine($"{obj.Object2.Id}"));
        }

        public void SelectElementInLink(Element elemntInLink, List<RevitLinkInstance> revitLinks)
        {
            var link = GetLinkByElement(elemntInLink, revitLinks);
            if (link is null) { return; }
            try
            {
                var e1 = link.GetLinkDocument().GetElement(elemntInLink.Id);
                var ref1 = new Reference(e1);
                var stableRepresentation = ref1?.CreateLinkReference(link).ConvertToStableRepresentation(_doc); // здесь doc - ОСНОВНОЙ документ
                string fixedStableRepresentation = stableRepresentation.Replace(":RVTLINK", ":0:RVTLINK");
                var reference2 = Reference.ParseFromStableRepresentation(_doc, fixedStableRepresentation);
                _uiDoc.Selection.PickObjects(ObjectType.LinkedElement, new NoSelectionFilter(), "select", new Reference[] { reference2 });
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
            }

        }

        private RevitLinkInstance GetLinkByElement(Element e, List<RevitLinkInstance> revitLinks)
        {
            return revitLinks.FirstOrDefault(x => x.GetLinkDocument().Title == e.Document.Title);
        }

    }
}
