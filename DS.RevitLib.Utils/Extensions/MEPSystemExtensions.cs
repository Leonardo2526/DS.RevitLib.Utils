using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="MEPSystem"/>.
    /// </summary>
    public static class MEPSystemExtensions
    {
        /// <summary>
        /// Get elements from <paramref name="mEPSystem"/> by <paramref name="exculdedElementCategories"/> filter.
        /// </summary>
        /// <param name="mEPSystem"></param>
        /// <param name="exculdedElementCategories"></param>
        /// <returns>Returns all model elements in <paramref name="mEPSystem"/>.</returns>
        public static List<Element> GetElements(this MechanicalSystem mEPSystem, List<BuiltInCategory> exculdedElementCategories = null)
        {
            var result = new List<Element>();

            var elems = mEPSystem.DuctNetwork.GetEnumerator();
            while (elems.MoveNext()) { result.Add((Element)elems.Current); }

            if(exculdedElementCategories is null || !exculdedElementCategories.Any()) { return result; }
            else
            {
                var exludedCategoriesIds = exculdedElementCategories?.Select(obj => (int)obj);
                var categories = mEPSystem.Document.Settings.Categories.Cast<Category>().
                    Where(x => x.CategoryType == CategoryType.Model).Select(x => x.Id);
                if (exludedCategoriesIds is not null && exludedCategoriesIds.Any())
                {
                    categories = categories.Where(x => !exludedCategoriesIds.Contains(x.IntegerValue));
                }
                var filter = new ElementMulticategoryFilter(categories.ToList());

                return new FilteredElementCollector(mEPSystem.Document, result.Select(obj => obj.Id).ToList()).
                        WherePasses(filter).Cast<Element>().ToList();
            }
        }
    }
}
