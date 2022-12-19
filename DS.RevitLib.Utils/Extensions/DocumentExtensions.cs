using Autodesk.Revit.DB;
using iUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Document"/>.
    /// </summary>
    public static class DocumentExtensions
    {
        /// <summary>
        /// Get all <see cref="GeometryData"/> from <paramref name="doc"/>.
        /// </summary>
        /// <param name="doc">Current <see cref="Document"/>.</param>
        /// <param name="exludedCathegories">Excluded elements list of <see cref="Autodesk.Revit.DB.BuiltInCategory"/>.</param>
        /// <param name="tr"></param>
        /// <returns></returns>
        public static List<GeometryData> GetGeometryData(this Document doc, List<BuiltInCategory> exludedCathegories = null, Transform tr = null)
        {
            if (doc == null || !doc.IsValidObject)
                return new List<GeometryData>();
            var categories = doc.Settings.Categories.Cast<Category>().Where(x => x.CategoryType == CategoryType.Model).Select(x => x.Id)
                .Where(x => !x.IntegerValue.Equals((int)BuiltInCategory.OST_Materials)).ToList();
            var filter = new ElementMulticategoryFilter(categories);
            var geomModelElems = new FilteredElementCollector(doc).WhereElementIsNotElementType()
                    .WherePasses(filter).Where(x => x.IsGeometryElement())
                    .Select(x => new GeometryData(x, tr, false));

            exludedCathegories ??= new List<BuiltInCategory>() { BuiltInCategory.OST_TelephoneDevices };
            foreach (var category in exludedCathegories)
            {
                geomModelElems = geomModelElems.Where(x => x.Element.Category.GetBuiltInCategory() != category);
            }

            return geomModelElems.ToList();
        }

        /// <summary>
        /// Get all geometry elements from <paramref name="doc"/>.
        /// </summary>
        /// <param name="doc">Current <see cref="Document"/>.</param>
        /// /// <param name="exludedCathegories">Excluded elements list of <see cref="Autodesk.Revit.DB.BuiltInCategory"/>.</param>
        /// <param name="tr"></param>
        /// <returns>Returns all elements with solid volume.</returns>
        public static List<Element> GetElements(this Document doc, List<BuiltInCategory> exludedCathegories = null, Transform tr = null)
        {
            var geomModelElems = GetGeometryData(doc, exludedCathegories, tr);
            return geomModelElems.Select(obj => obj.Element).ToList();
        }

        /// <summary>
        /// Get all loaded links in <see cref="Document"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Retruns null if no loaded links are in document.</returns>
        public static List<RevitLinkInstance> GetLoadedLinks(this Document doc)
        {
            var allLinks = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList();
            if (allLinks is null || !allLinks.Any()) return null;

            var loadedLinks = new List<RevitLinkInstance>();
            foreach (var link in allLinks)
            {
                RevitLinkType type = doc.GetElement(link.GetTypeId()) as RevitLinkType;
                bool loaded = RevitLinkType.IsLoaded(doc, type.Id);
                if (loaded)
                {
                    loadedLinks.Add(link);
                }
            }

            return loadedLinks;
        }
    }
}
