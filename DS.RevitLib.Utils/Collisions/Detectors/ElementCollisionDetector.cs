﻿using Autodesk.Revit.DB;
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
            Solid checkTransformedSolid = GetCheckSolid(checkObject1);

            var intersection = new ElementsIntersection(_checkObjects2, _checkObjects2Doc);

            List<Element> elements = checkTransformedSolid is null ?
                intersection.GetIntersectedElements(checkObject1, exludedCheckObjects2) :
                intersection.GetIntersectedElements(checkTransformedSolid, exludedCheckObjects2);

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

        /// <summary>
        /// Transform solid of <paramref name="checkObject1"/> if it belongs to transformed <see cref="RevitLinkInstance"/>.
        /// </summary>
        /// <param name="checkObject1"></param>
        /// <returns>Returns null if no link or transform in link exist.</returns>
        private Solid GetCheckSolid(Element checkObject1)
        {
            if (_revitLink is not null)
            {
                var linkTransform = _revitLink.GetTotalTransform();
                if (!linkTransform.AlmostEqual(Transform.Identity))
                {
                    Solid solid = ElementUtils.GetSolid(checkObject1);
                    return SolidUtils.CreateTransformed(solid, linkTransform.Inverse);
                }
            }

            return null;
        }

    }
}
