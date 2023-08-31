using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions;
using DS.RevitLib.Utils.Collisions.Detectors;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements.MEPElements.Transfer
{
    /// <summary>
    /// An object that represents builder to check collisions. 
    /// </summary>
    public class CollisionCheckerBuilder
    {
        private readonly Document _doc;
        private readonly List<Element> _docElements;
        private readonly Dictionary<RevitLinkInstance, List<Element>> _linkElementsDict;

        /// <summary>
        /// Instantiate an object to check collisions in bounding box built by <paramref name="pointsForBB"/> with specified <paramref name="offset"/>. 
        /// </summary>
        public CollisionCheckerBuilder(List<Element> docElements, Dictionary<RevitLinkInstance, List<Element>> LinksElements,
            List<XYZ> pointsForBB, double offset)
        {
            _doc = docElements.FirstOrDefault().Document;
            _docElements = docElements;
            _linkElementsDict = LinksElements;

            BoundingBoxXYZ boxXYZ = ElementUtils.GetBoundingBox(pointsForBB, offset);

            CollisionDetector = GetDetector(boxXYZ);
        }

        /// <summary>
        /// Get collsion checkers.
        /// </summary>
        public ISolidCollisionDetector CollisionDetector { get; private set; }

        private ISolidCollisionDetector GetDetector(BoundingBoxXYZ boxXYZ)
        {
            //boxXYZ.Show(_doc);
            var transform = boxXYZ.Transform;
            var outline = new Outline(transform.OfPoint(boxXYZ.Min), transform.OfPoint(boxXYZ.Max));

            List<BuiltInCategory> exludedCathegories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_TelephoneDevices,
                BuiltInCategory.OST_Materials,
                BuiltInCategory.OST_Rooms
            };
            (var docElements, var linkElementsDict) = new ElementsExtractor(_doc, exludedCathegories, outline).GetAll();

            return new SolidElementCollisionDetectorFactory(_doc, docElements, linkElementsDict);
        }


        private List<Element> GetElementsInBB(BoundingBoxXYZ boxXYZ, List<Element> excludedObjects)
        {
            var outline = new Outline(boxXYZ.Min, boxXYZ.Max);
            List<RevitLinkInstance> links = _linkElementsDict?.Select(obj => obj.Key).ToList();

            var bBCollisionUtils = new BBCollisionUtils(_doc, _docElements, links);
            return bBCollisionUtils.GetElements(outline, 0, excludedObjects);
        }
    }
}
