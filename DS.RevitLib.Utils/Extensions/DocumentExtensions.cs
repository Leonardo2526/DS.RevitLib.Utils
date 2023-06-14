﻿using Autodesk.Revit.DB;
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
        public static List<GeometryData> GetGeometryData(this Document doc,
            List<BuiltInCategory> exludedCathegories = null, Transform tr = null)
        {
            if (doc == null || !doc.IsValidObject)
                return new List<GeometryData>();

            var exludedCategoriesIds = exludedCathegories?.Select(obj => (int)obj);
            var categories = doc.Settings.Categories.Cast<Category>().
                Where(x => x.CategoryType == CategoryType.Model).
                Select(x => x.Id).
                Where(x => !exludedCategoriesIds.Contains(x.IntegerValue)).
                ToList();

            var filter = new ElementMulticategoryFilter(categories);
            var geomModelElems = new FilteredElementCollector(doc).
                WhereElementIsNotElementType().
                WherePasses(filter).
                Where(x => x.IsGeometryElement()).
                Select(x => new GeometryData(x, tr, false));

            return geomModelElems.ToList();
        }

        /// <summary>
        /// Get all <see cref="Autodesk.Revit.DB.Element"/>'s with geometry from <paramref name="doc"/>.
        /// </summary>
        /// <param name="doc">Current <see cref="Document"/>.</param>
        /// <param name="elementIds">Input elements ids to get geometry elements.</param>
        /// <param name="exludedCathegories">Excluded elements list of <see cref="Autodesk.Revit.DB.BuiltInCategory"/>.</param>
        /// <returns>
        /// List of <see cref="Autodesk.Revit.DB.Element"/> with geometry.
        /// <para>
        /// If <paramref name="elementIds"/> is null or empty returns all geometry elements in <paramref name="doc"/>.
        /// </para>
        /// </returns>
        public static List<Element> GetGeometryElements(this Document doc, List<BuiltInCategory> exludedCathegories = null, List<ElementId> elementIds = null)
        {
            if (doc == null || !doc.IsValidObject)
                return new List<Element>();

            var exludedCategoriesIds = exludedCathegories?.Select(obj => (int)obj);
            var categories = doc.Settings.Categories.Cast<Category>().
                Where(x => x.CategoryType == CategoryType.Model).Select(x => x.Id);
            if (exludedCategoriesIds is not null && exludedCategoriesIds.Any())
            {
                categories = categories.Where(x => !exludedCategoriesIds.Contains(x.IntegerValue));
            }

            var filter = new ElementMulticategoryFilter(categories.ToList());

            IEnumerable<Element> geomModelElems = new List<Element>();
            if (elementIds is null || elementIds.Count == 0)
            {
                geomModelElems = new FilteredElementCollector(doc).
                    WhereElementIsNotElementType().
                    WherePasses(filter).
                    Where(x => x.IsGeometryElement());
            }
            else
            {
                geomModelElems = new FilteredElementCollector(doc, elementIds).
                   WhereElementIsNotElementType().
                   WherePasses(filter).
                   Where(x => x.IsGeometryElement());
            }

            return geomModelElems.ToList();
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


        /// <summary>
        /// Get all <paramref name="doc"/> MEPSystems of <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="doc"></param>
        /// <returns>Returns list of <see cref="MEPSystem"/>'s of <paramref name="doc"/>.
        /// <para>
        /// Returns <see langword="null"/> if no systems of <typeparamref name="T"/> type was found.
        /// </para>
        /// </returns>
        public static List<T> GetMEPSystems<T>(this Document doc) where T : MEPSystem
        {
            return new FilteredElementCollector(doc).OfClass(typeof(T))?.Cast<T>().ToList();
        }

        /// <summary>
        /// Specifies whether current <see cref="Document"/>'s state is in Revit context.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Returns <see langword="true"/> if transactions are available. Otherwise returns <see langword="false"/>.</returns>
        public static bool IsRevitContext(this Document doc)
        {
            try
            {
                var tr = new Transaction(doc, "CheckContext");
                var st = tr.GetStatus();
                tr.Start();
                tr.RollBack();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
