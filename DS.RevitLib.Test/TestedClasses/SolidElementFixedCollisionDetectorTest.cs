using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Collisions;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Elements;
using iUtils.SelctionFilters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class SolidElementFixedCollisionDetectorTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        public SolidElementFixedCollisionDetectorTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
        }

        public void Run()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            Element element = _doc.GetElement(reference);
            Solid solid = ElementUtils.GetSolid(element);

            var extractor = new ElementsExtractor(_doc);
            var (docElements, linkElementsDict) = extractor.GetAll();

            var modelElements = new List<Element>();
            modelElements.AddRange(docElements);
            if (linkElementsDict is not null && linkElementsDict.Any())
            {
                List<Element> linkElements = linkElementsDict.SelectMany(obj => obj.Value).ToList();
                modelElements.AddRange(linkElements);
            }

            var excluded = new List<Element>() { element };

            var detector = new SolidElementFixedCollisionDetector(_doc, modelElements, excluded);
            var collisions = detector.GetCollision(solid).Cast<SolidElementCollision>().ToList();

            var collisionElemIds = collisions.Select(obj => obj.Object2.Id).ToList();
            //collisions.ForEach(collision => _uiDoc.Selection.SetElementIds(collisionElemIds));
            //_uiDoc.RefreshActiveView();

            foreach (var item in linkElementsDict)
            {
                var linkDetector = new SolidElementFixedCollisionLinkDetector(_doc, item, excluded);
                //linkDetector.ShowLinkSolids();
                //_uiDoc.RefreshActiveView();
                var linkCollisions = linkDetector.GetCollision(solid).Cast<SolidElementCollision>().ToList();
                collisions.AddRange(linkCollisions);
            }

            collisionElemIds = collisions.Select(obj => obj.Object2.Id).ToList();
            collisions.ForEach(collision => SelectElementInLink(collision.Object2, linkElementsDict.Keys.ToList()));

            Debug.WriteLine($"Collisions count is {collisions.Count}");
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
