using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Models
{
    internal class ElementsIntersection
    {
        /// <summary>
        /// Objects to check collisions.
        /// </summary>
        protected readonly List<Element> _checkObjects2;

        /// <summary>
        /// Document of checkObjects2 used for <see cref="Autodesk.Revit.DB.FilteredElementCollector"/>;
        /// </summary>
        protected readonly Document _checkObjects2Doc;

        public ElementsIntersection(List<Element> checkObjects2, Document checkObjects2Doc)
        {
            _checkObjects2 = checkObjects2;
            _checkObjects2Doc = checkObjects2Doc;
        }


        /// <summary>
        /// Get elements in checkObjects2 that intersect <paramref name="checkSolid"/>.
        /// </summary>
        /// <param name="checkSolid"></param>
        /// <param name="exludedCheckObjects2"></param>
        /// <returns>Returns elements that intersect <paramref name="checkSolid"/>.</returns>
        public List<Element> GetIntersectedElements(Solid checkSolid, List<Element> exludedCheckObjects2 = null)
        {
            var collector = new FilteredElementCollector(_checkObjects2Doc, _checkObjects2.Select(el => el.Id).ToList());

            //apply quick filter.
            BoundingBoxXYZ boxXYZ = checkSolid.GetBoundingBox(); 
            ApplyQuickFilter(collector, boxXYZ);

            //apply exculsionFilter filter.
            if (exludedCheckObjects2 is not null && exludedCheckObjects2.Any())
            { collector.WherePasses(GetExclusionFilter(exludedCheckObjects2)); };

            //apply slow filter
            return collector.WherePasses(new ElementIntersectsSolidFilter(checkSolid)).ToElements().ToList();
        }


        /// <summary>
        /// Get elements in checkObjects2 that intersect <paramref name="checkElement"/>.
        /// </summary>
        /// <param name="checkElement"></param>
        /// <param name="exludedCheckObjects2"></param>
        /// <returns>Returns elements that intersect <paramref name="checkElement"/>.</returns>
        public List<Element> GetIntersectedElements(Element checkElement, List<Element> exludedCheckObjects2 = null)
        {
            var collector = new FilteredElementCollector(_checkObjects2Doc, _checkObjects2.Select(el => el.Id).ToList());

            //apply quick filter.
            BoundingBoxXYZ boxXYZ = checkElement.get_BoundingBox(null);
            ApplyQuickFilter(collector, boxXYZ);

            //apply exculsionFilter filter.
            if (exludedCheckObjects2 is not null && exludedCheckObjects2.Any())
            { collector.WherePasses(GetExclusionFilter(exludedCheckObjects2)); };

            //apply slow filter
            return collector.WherePasses(new ElementIntersectsElementFilter(checkElement)).ToElements().ToList();
        }

        private void ApplyQuickFilter(FilteredElementCollector collector, BoundingBoxXYZ boxXYZ)
        {
            var transform = boxXYZ.Transform;
            var outline = new Outline(transform.OfPoint(boxXYZ.Min), transform.OfPoint(boxXYZ.Max));
            collector.WherePasses(new BoundingBoxIntersectsFilter(outline, 0));
        }


        private ExclusionFilter GetExclusionFilter(List<Element> excludedElements)
        {
            var excludedElementsIds = excludedElements?.Select(el => el.Id).ToList();

            Document doc = excludedElements.First().Document;
            var excludedElementsInsulationIds = new List<ElementId>();
                excludedElements.ForEach(obj =>
            {
                Element insulation = InsulationLiningBase.GetInsulationIds(doc, obj.Id)?
                  .Select(x => doc.GetElement(x)).FirstOrDefault();
                if (insulation != null) { excludedElementsInsulationIds.Add(insulation.Id); }
            });
            excludedElementsIds.AddRange(excludedElementsInsulationIds);

            return excludedElementsIds is null || !excludedElementsIds.Any() ?
                 null : new ExclusionFilter(excludedElementsIds);
        }
    }
}
