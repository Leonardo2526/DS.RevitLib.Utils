using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Collisions2;
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
    internal class ElementCollisionDetectorTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private ElementCollisionDetectorFactory _factory;

        public ElementCollisionDetectorTest(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;
        }

        public void RunWithFactory()
        {
            Reference reference1 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element");
            Element element1 = _doc.GetElement(reference1);

            var checkObjects2ToExclude = new List<Element>() { element1 };

            var (docElements, linkElementsDict) = new ElementsExtractor(_doc).GetAll();
            _factory = new ElementCollisionDetectorFactory(_doc, docElements, linkElementsDict);
            var collisions = _factory.GetCollisions(element1, checkObjects2ToExclude).Cast<ElementCollision>().ToList();

            Debug.WriteLine($"\nCollisions count is {collisions.Count}");
            Debug.WriteLine($"Collision objects ids:");
            collisions.ForEach(obj => Debug.WriteLine($"{obj.Object1.Id} - {obj.Object2.Id}"));
        }

        public void RepeatRun()
        {
            Reference reference1 = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element2");
            Element element1 = _doc.GetElement(reference1);
            Solid solid1 = ElementUtils.GetSolid(element1);

            var checkObjects2ToExclude = new List<Element>() { element1 };
            var collisions = _factory.GetCollisions(element1, checkObjects2ToExclude).Cast<ElementCollision>().ToList();

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
