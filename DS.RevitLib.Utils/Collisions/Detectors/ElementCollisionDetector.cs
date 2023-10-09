using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.ClassLib.VarUtils.Collisions;
using System.Collections.Generic;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Extensions;

namespace DS.RevitLib.Utils.Collisions.Detectors
{
    /// <inheritdoc/>
    public class ElementCollisionDetector : CollisionDetector<Element, Element>
    {
        /// <inheritdoc/>
        public ElementCollisionDetector(Document doc, List<Element> checkObjects2) :
            base(doc, checkObjects2)
        { }

        /// <inheritdoc/>
        public ElementCollisionDetector(RevitLinkInstance revitLink,
            List<Element> checkLinkObjects) :
            base(revitLink, checkLinkObjects)
        { }

        /// <inheritdoc/>
        public override List<(Element, Element)> GetCollisions(Element checkObject1, List<Element> exludedCheckObjects2 = null)
        {
            var intersection = new ElementsIntersection(_doc, _checkObjects2, _checkObjects2Doc);
            List<Element> elements = intersection.GetIntersectedElements(checkObject1, exludedCheckObjects2);

            var collisions = new List<(Element, Element)>();

            foreach (Element element in elements)
            {
                var collision = (checkObject1, element);
                if (MinVolume != 0)
                {
                    if (collision.GetIntersectionSolid(MinVolume) != null)
                    { collisions.Add(collision); }
                }
                else collisions.Add(collision);
            }

            return collisions;
        }
    }
}
