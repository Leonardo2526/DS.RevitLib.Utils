using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Elements.MEPElements.Transfer
{
    /// <summary>
    /// An object that represents builder to check collisions. 
    /// </summary>
    public class CollisionCheckerBuilder
    {
        private readonly Document _doc;
        private readonly List<Element> _docElements;
        private readonly Dictionary<RevitLinkInstance, List<Element>> _linksElements;

        /// <summary>
        /// Instantiate an object to check collisions in bounding box built by <paramref name="pointsForBB"/> with specified <paramref name="offset"/>. 
        /// </summary>
        public CollisionCheckerBuilder(List<Element> docElements, Dictionary<RevitLinkInstance, List<Element>> LinksElements, 
            List<XYZ> pointsForBB, double offset, List<Element> excludedElements)
        {
            _doc = docElements.FirstOrDefault().Document;
            _docElements = docElements;
            _linksElements = LinksElements;

            BoundingBoxXYZ boxXYZ = ElementUtils.GetBoundingBox(pointsForBB, offset);

            CollisionCheckers = GetCollisionCheckers(boxXYZ, excludedElements);
        }

        /// <summary>
        /// Get collsion checkers.
        /// </summary>
        public List<ICollisionChecker> CollisionCheckers { get; private set; }

        private List<ICollisionChecker> GetCollisionCheckers(BoundingBoxXYZ boxXYZ, List<Element> excludedElements)
        {
            var collisionCheckers = new List<ICollisionChecker>();

            var elementsInOutlineIds = GetElementsInBB(boxXYZ, excludedElements).Select(obj => obj.Id);
            if (!elementsInOutlineIds.Any()) { return collisionCheckers; }

            //Get model checker
            List<Element> modelElements = _docElements.
                Where(obj => elementsInOutlineIds.Contains(obj.Id)).ToList();
            collisionCheckers.Add(new SolidCollisionChecker(modelElements, excludedElements));

            //get link checkers
            foreach (var link in _linksElements)
            {
                collisionCheckers.Add(new LinkCollisionChecker(link.Value, link.Key, null));
            }

            return collisionCheckers;
        }


        private List<Element> GetElementsInBB(BoundingBoxXYZ boxXYZ, List<Element> excludedObjects)
        {
            var outline = new Outline(boxXYZ.Min, boxXYZ.Max);
            List<RevitLinkInstance> links = _linksElements?.Select(obj => obj.Key).ToList();
            List<Element> modelElements = _linksElements?.SelectMany(obj => obj.Value).ToList();

            var bBCollisionUtils = new BBCollisionUtils(_doc, modelElements, links);
            return bBCollisionUtils.GetElements(outline, 0, excludedObjects);
        }
    }
}
