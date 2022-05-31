using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
