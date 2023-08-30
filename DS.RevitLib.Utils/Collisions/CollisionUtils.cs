using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitLib.Utils
{
    public static class CollisionUtils
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

        /// <summary>
        /// Find intersection <see cref="Solid"/> between <paramref name="element1"/> and <paramref name="element2"/> with 
        /// specified minimum <paramref name="minVolume"/> value.
        /// </summary>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        /// <param name="solid1"><paramref name="element1"/>'s <see cref="Solid"/>.</param>
        /// <param name="solid2"><paramref name="element2"/>'s <see cref="Solid"/>.</param>
        /// <param name="minVolume">Minimum intersection <see cref="Solid"/> volume.</param>
        /// <returns>Intersection <see cref="Solid"/>.
        /// <para>
        /// Returns <see langword="null"/> if intersection volume is less than <paramref name="minVolume"/>.
        /// </para>
        /// <para>
        /// Returns <see langword="null"/> if calculation of intersection <see cref="Solid"/> was <see langword="null"/> or failed.
        /// </para>
        /// </returns>
        /// <exception cref="Exception"></exception>
        public static Solid GetIntersectionSolid(Element element1, Element element2, out Solid solid1, out Solid solid2, double minVolume = 0)
        {
            solid1 = element1.Document.IsLinked ?
                element1.GetTransformed(element1.GetLink(element2.Document)) :
                ElementUtils.GetSolid(element1);
            solid2 = element2.Document.IsLinked ? 
                element2.GetTransformed(element2.GetLink(element1.Document)) : 
                ElementUtils.GetSolid(element2);

            Solid intersectionSolid = null;

            try
            {
                intersectionSolid =
                    BooleanOperationsUtils.ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Intersect);
                if (intersectionSolid is null || intersectionSolid.Volume == 0 || intersectionSolid.Volume < minVolume)
                {
                    //string txt = "Elements have no intersections";
                    //Debug.WriteLine(txt);
                    return null;
                }               
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return intersectionSolid;
        }
    }
}
