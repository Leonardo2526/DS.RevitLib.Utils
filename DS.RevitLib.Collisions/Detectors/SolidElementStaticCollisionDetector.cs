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
    public class SolidElementStaticCollisionDetector : StaticCollisionDetector<Solid, Element>
    {
        private readonly ExclusionFilter _exculsionFilter;

        /// <inheritdoc/>
        public SolidElementStaticCollisionDetector(Document doc, List<Element> checkObjects2,
            List<Element> checkObjects2ToExclude = null) :
            base(doc, checkObjects2, checkObjects2ToExclude)
        {
            _exculsionFilter = GetExclusionFilter(checkObjects2ToExclude);
        }

        /// <inheritdoc/>
        public SolidElementStaticCollisionDetector(RevitLinkInstance revitLink,
            List<Element> checkLinkObjects, List<Element> exludedElementsInLink = null) :
            base(revitLink, checkLinkObjects, exludedElementsInLink)
        {
            _exculsionFilter = GetExclusionFilter(exludedElementsInLink);
        }

        /// <inheritdoc/>
        public override List<IBestCollision> GetCollision(Solid checkObject1)
        {
            Solid checkSolid = GetCheckSolid(checkObject1);

            List<Element> elements = GetIntersectedElements(checkSolid);

            var collisions = new List<IBestCollision>();
            elements.ForEach(obj => collisions.Add(new SolidElementCollision(checkSolid, obj)));

            return collisions;
        }

        /// <inheritdoc/>
        public override List<IBestCollision> GetCollisions(List<Solid> checkObjects1)
        {
            var collisions = new List<IBestCollision>();
            checkObjects1.ForEach(obj => collisions.AddRange(GetCollision(obj)));
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
        /// <returns>Returns elements thst intersect <paramref name="checkSolid"/>.</returns>
        private List<Element> GetIntersectedElements(Solid checkSolid)
        {
            var collector = new FilteredElementCollector(_checkObjects2Doc, _checkObjects2.Select(el => el.Id).ToList());

            //apply quick filter.
            BoundingBoxXYZ boxXYZ = checkSolid.GetBoundingBox();
            var transform = boxXYZ.Transform;
            var outline = new Outline(transform.OfPoint(boxXYZ.Min), transform.OfPoint(boxXYZ.Max));
            collector = collector.WherePasses(new BoundingBoxIntersectsFilter(outline, 0));

            //apply exculsionFilter filter.
            if (_exculsionFilter is not null) { collector = collector.WherePasses(_exculsionFilter); };

            //apply slow filter
            return collector.WherePasses(new ElementIntersectsSolidFilter(checkSolid)).ToElements().ToList();
        }
    }
}
