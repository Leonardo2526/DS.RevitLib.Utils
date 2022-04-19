using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils
{
    public static class Collision
    {
        /// <summary>
        /// Check collisions between elements and one another element with exluded as option.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="elements"></param>
        /// <param name="excludedElements"></param>
        /// <returns>Returns list of collision elements</returns>
        public static IList<Element> GetByElements(Element element, ICollection<Element> elements, ICollection<Element> excludedElements = null)
        {
            ICollection<ElementId> elementsIds = elements.Select(el => el.Id).ToList();
            FilteredElementCollector newCollector = new FilteredElementCollector(element.Document, elementsIds);

            ElementIntersectsElementFilter elementIntersectsElementFilter = new ElementIntersectsElementFilter(element);

            var collisionElements = newCollector.WherePasses(elementIntersectsElementFilter).ToElements();

            if (excludedElements is not null && excludedElements.Count > 0)
            {
                var exclusionFilter = GetExculsionFilter(excludedElements);
                collisionElements = newCollector.WherePasses(exclusionFilter).ToElements();
            }

            return collisionElements;
        }

        private static ExclusionFilter GetExculsionFilter(ICollection<Element> excludedElements)
        {
            ICollection<ElementId> excludedElementsIds = excludedElements.Select(el => el.Id).ToList();
            return new ExclusionFilter(excludedElementsIds);
        }
    }
}
