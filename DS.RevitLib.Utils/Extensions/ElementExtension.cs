using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Extensions
{
    public static class ElementExtension
    {
        /// <summary>
        /// Check if object is null or valid
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Return false if object is null or not valid. Return true if object isn't null and valid</returns>
        public static bool NotNullValidObject(this Element element)
        {
            if (element is null || !element.IsValidObject)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get ElementType object.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Return ElementType object.</returns>
        public static ElementType GetElementType2(this Element element)
        {
            ElementId id = element.GetTypeId();
            return element.Document.GetElement(id) as ElementType;
        }

        /// <summary>
        /// Order elements list by base point.
        /// </summary>
        /// <param name="basePoint"></param>
        /// <returns>Return ordered elements by descending distances from location points to base point.</returns>
        public static List<Element> OrderByPoint(this List<Element> elements, XYZ basePoint)
        {
            var distances = new Dictionary<double, Element>();

            foreach (var elem in elements)
            {
                XYZ point = ElementUtils.GetLocationPoint(elem);
                double distance = basePoint.DistanceTo(point);
                distances.Add(distance, elem);
            }

            distances = distances.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            return distances.Values.ToList();
        }

        /// <summary>
        /// Get elements without spuds
        /// </summary>
        /// <param name="elements"></param>
        /// <returns>Return list of elements without spuds.</returns>
        public static List<Element> ExludeSpudes(this List<Element> elements)
        {
            var roots = new List<Element>();

            foreach (var elem in elements)
            {
                if (!elem.IsSpud())
                {
                    roots.Add(elem);
                }
            }

            return roots.Any() ? roots : elements;
        }

        public static bool IsSpud(this Element element)
        {
            if (element is not FamilyInstance familyInstance)
            {
                return false;
            }

            var pt = ElementUtils.GetPartType(familyInstance);
            if (pt == PartType.SpudPerpendicular || pt == PartType.SpudAdjustable)
            {
                return true;
            }

            return false;
        }

    }
}
