using Autodesk.Revit.DB;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Collisions
{
    /// <inheritdoc/>
    public class SolidElementCollisionDetector : CollisionDetector<Solid, Element>
    {
        /// <inheritdoc/>
        public SolidElementCollisionDetector(Document doc, List<Element> checkObjects2) :
            base(doc, checkObjects2)
        { }

        /// <inheritdoc/>
        public SolidElementCollisionDetector(RevitLinkInstance revitLink,
            List<Element> checkLinkObjects) :
            base(revitLink, checkLinkObjects)
        { }

        /// <inheritdoc/>
        public override List<IBestCollision> GetCollisions(Solid checkObject1, List<Element> exludedCheckObjects2 = null)
        {
            Solid checkSolid = GetCheckSolid(checkObject1);

            List<Element> elements = GetIntersectedElements(checkSolid, exludedCheckObjects2);

            var collisions = new List<IBestCollision>();
            elements.ForEach(obj => collisions.Add(new SolidElementCollision(checkSolid, obj)));

            return collisions;
        }

        private ExclusionFilter GetExclusionFilter(List<Element> excludedElements)
        {
            var excludedElementsIds = excludedElements?.Select(el => el.Id).ToList();
            return excludedElementsIds is null || !excludedElementsIds.Any() ?
                 null : new ExclusionFilter(excludedElementsIds);
        }

        /// <summary>
        /// Transform solid if it belongs to transformed <see cref="RevitLinkInstance"/>.
        /// </summary>
        /// <param name="solid"></param>
        /// <returns>Returns the input <paramref name="solid"/> if no link or transform in link is exist.</returns>
        private Solid GetCheckSolid(Solid solid)
        {
            if (_revitLink is not null)
            {
                var linkTransform = _revitLink.GetTotalTransform();
                return linkTransform.AlmostEqual(Transform.Identity) ?
                    solid : SolidUtils.CreateTransformed(solid, linkTransform.Inverse);
            }

            return solid;
        }

        /// <summary>
        /// Get elements in checkObjects2 that intersect <paramref name="checkSolid"/>.
        /// </summary>
        /// <param name="checkSolid"></param>
        /// <param name="exludedCheckObjects2"></param>
        /// <returns>Returns elements thst intersect <paramref name="checkSolid"/>.</returns>
        private List<Element> GetIntersectedElements(Solid checkSolid, List<Element> exludedCheckObjects2 = null)
        {
            var collector = new FilteredElementCollector(_checkObjects2Doc, _checkObjects2.Select(el => el.Id).ToList());

            //apply quick filter.
            BoundingBoxXYZ boxXYZ = checkSolid.GetBoundingBox();
            var transform = boxXYZ.Transform;
            var outline = new Outline(transform.OfPoint(boxXYZ.Min), transform.OfPoint(boxXYZ.Max));
            collector.WherePasses(new BoundingBoxIntersectsFilter(outline, 0));

            //apply exculsionFilter filter.
            if (exludedCheckObjects2 is not null && exludedCheckObjects2.Any())
            { collector.WherePasses(GetExclusionFilter(exludedCheckObjects2)); };

            //apply slow filter
            return collector.WherePasses(new ElementIntersectsSolidFilter(checkSolid)).ToElements().ToList();
        }
    }
}
