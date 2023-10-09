using Autodesk.Revit.DB;
using iUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        /// Get all <see cref="Autodesk.Revit.DB.Element"/>'s with geometry from <paramref name="modelDoc"/> or <paramref name="link"/>.
        /// </summary>
        /// <param name="doc">Current <see cref="Document"/>.</param>
        /// <param name="link"></param>
        /// <param name="elementIds">Input elements ids to get geometry elements.</param>
        /// <param name="exludedCathegories">Excluded elements list of <see cref="Autodesk.Revit.DB.BuiltInCategory"/>.</param>
        /// <param name="includeLines"></param>
        /// <param name="outline">Specify if found elemens should be filered by inside <see cref="Autodesk.Revit.DB.Outline"/>.</param>
        /// <returns>
        /// List of <see cref="Autodesk.Revit.DB.Element"/> with geometry from <paramref name="doc"/> if <paramref name="link"/> is null.
        /// <para>
        /// List of <see cref="Autodesk.Revit.DB.Element"/> with geometry from <paramref name="link"/> if it isn't null.
        /// </para>
        /// <para>
        /// If <paramref name="includeLines"/> parameter is <see langword="true"/> model <see cref="Line"/>'s can be included to list.     
        /// </para>    
        /// <para>
        /// If <paramref name="elementIds"/> is null or empty returns all geometry elements in <paramref name="doc"/>.
        /// </para>
        /// </returns>
        public static List<Element> GetGeometryElements(
            this Document doc,
            RevitLinkInstance link = null,
            List<BuiltInCategory> exludedCathegories = null,
            List<ElementId> elementIds = null,
            bool includeLines = false, Outline outline = null)
        {
            Document modelDoc = link is null ? doc : link.GetLinkDocument();

            if (modelDoc == null || !modelDoc.IsValidObject)
                return new List<Element>();

            var exludedCategoriesIds = exludedCathegories?.Select(obj => (int)obj);
            var categories = modelDoc.Settings.Categories.Cast<Category>().
                Where(x => x.CategoryType == CategoryType.Model).Select(x => x.Id);
            if (exludedCategoriesIds is not null && exludedCategoriesIds.Any())
            {
                categories = categories.Where(x => !exludedCategoriesIds.Contains(x.IntegerValue));
            }

            var collector = elementIds is null || elementIds.Count == 0 ?
                new FilteredElementCollector(modelDoc) :
                new FilteredElementCollector(modelDoc, elementIds);

            var categoryFilter = new ElementMulticategoryFilter(categories.ToList());
            BoundingBoxIntersectsFilter boudingBoxFilter;
            if (link is null)
            { boudingBoxFilter = outline is null ? null : new BoundingBoxIntersectsFilter(outline); }
            else
            { boudingBoxFilter = outline is null ? null : outline.GetBoundingBoxFilter(link); }

            collector = boudingBoxFilter is null ?
               collector.WhereElementIsNotElementType().WherePasses(categoryFilter) :
               collector.WhereElementIsNotElementType().WherePasses(categoryFilter).WherePasses(boudingBoxFilter);

            var exclusionFilter = GetExclusion(modelDoc);
            if (exclusionFilter is not null) { collector.WherePasses(exclusionFilter); }

            var elems = collector.ToElements();
            return elems.Where(x => x.IsGeometryElement(includeLines)).ToList();

            static ExclusionFilter GetExclusion(Document modelDoc)
            {
                var ecollector = new FilteredElementCollector(modelDoc);

                var excludedTypes = new List<Type>()
                {
                    typeof(ImportInstance),
                    typeof(FilledRegion)
                };

                var filter = new ElementMulticlassFilter(excludedTypes);
                var excluded = ecollector.WherePasses(filter);
                if (excluded is null || !excluded.Any()) { return null; };

                var excludedIds = excluded.Select(e => e.Id).ToList();
                return new ExclusionFilter(excludedIds);
            }
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
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                Debug.WriteLine("Outside Revit context.");
                return false;
            }
        }

        /// <summary>
        /// Get geometry elements from <paramref name="doc"/> model that contain <paramref name="point"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="point"></param>
        /// <returns>
        /// Elements that contains <paramref name="point"/>.
        /// <para>
        /// Empty list if none <paramref name="doc"/> elements contain <paramref name="point"/>.    
        /// </para>
        /// </returns>
        public static List<Element> GetGeometryElements(this Document doc, XYZ point)
        {
            var elemensOnPoint = new List<Element>();

            var filter = new BoundingBoxContainsPointFilter(point);
            var collector = new FilteredElementCollector(doc);
            var elements = collector.
                WherePasses(filter).
                ToElements().
                Where(el => el.IsGeometryElement()).
                ToList();

            if (!elements.Any()) { return elemensOnPoint; }

            //Specify collision object
            elements.ForEach(obj =>
            {
                var solid = ElementUtils.GetSolid(obj);
                if (solid.Contains(point)) { elemensOnPoint.Add(obj); }
            });

            return elemensOnPoint;
        }

        /// <summary>
        /// Get elements of <paramref name="types"/> from <see cref="Document"/> and all loaded links.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="types"></param>
        /// <param name="outline"></param>
        /// <returns>
        /// All elements of <paramref name="types"/>.
        /// Returns empty list if no elements of <paramref name="types"/> were deteted.
        /// </returns>
        public static List<Element> GetElementsOfType(this Document doc, List<Type> types, Outline outline = null)
        {
            var elementsOfTypes = new List<Element>();

            var docFloors = GetFromDoc(types, doc, null, outline);
            elementsOfTypes.AddRange(docFloors);

            var linkFloors = GetFromLinks(types, doc, outline);
            elementsOfTypes.AddRange(linkFloors);

            return elementsOfTypes;

            List<Element> GetFromDoc(IList<Type> types, Document currentDoc, RevitLinkInstance link = null, Outline outline = null)
            {
                var collector = new FilteredElementCollector(currentDoc);

                BoundingBoxIntersectsFilter boudingBoxFilter;
                if (link is null)
                { boudingBoxFilter = outline is null ? null : new BoundingBoxIntersectsFilter(outline); }
                else
                { boudingBoxFilter = outline?.GetBoundingBoxFilter(link); }
                if (boudingBoxFilter is not null) { collector = collector.WherePasses(boudingBoxFilter); }

                var classFilter = new ElementMulticlassFilter(types);
                collector = collector.WherePasses(classFilter);
                return collector.ToElements().ToList();
            }

            List<Element> GetFromLinks(List<Type> types, Document currentDoc, Outline outline)
            {
                var elements = new List<Element>();

                var allLinks = currentDoc.GetLoadedLinks();
                if (allLinks is null || !allLinks.Any()) return elements;

                foreach (var link in allLinks)
                {
                    Document linkDoc = link.GetLinkDocument();
                    var linkFloors = GetFromDoc(types, linkDoc, link, outline);
                    elements.AddRange(linkFloors);
                }

                return elements;
            }
        }

        /// <summary>
        /// Get floors from <see cref="Document"/> and all loaded links.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="outline"></param>
        /// <returns>
        /// All floors.
        /// Returns empty list if no floors were deteted.
        /// </returns>
        public static List<Element> GetFloors(this Document doc, Outline outline = null)
        {
            var types = new List<Type>() { typeof(Floor) };
            return GetElementsOfType(doc, types, outline);

        }

        /// <summary>
        /// Get floors from <see cref="Document"/> and all loaded links.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="outline"></param>
        /// <returns>
        /// All floors.
        /// Returns empty list if no floors were deteted.
        /// </returns>
        public static List<Element> GetCeilings(this Document doc, Outline outline = null)
        {
            var types = new List<Type>() { typeof(Floor), typeof(Ceiling), typeof(RoofBase) };
            return GetElementsOfType(doc, types, outline);
        }

        /// <summary>
        /// Try to get <see cref="RevitLinkInstance"/> from <paramref name="checkDoc"/>.
        /// </summary>
        /// <param name="activeDoc"></param>
        /// <param name="checkDoc"></param>
        /// <returns>
        /// <see cref="RevitLinkInstance"/> if <paramref name="checkDoc"/> is loaded link in <paramref name="activeDoc"/> document.
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static RevitLinkInstance TryGetLink(this Document activeDoc, Document checkDoc)
        {
            if (!checkDoc.IsLinked) { return null; }
            else
            {
                var links = activeDoc.GetLoadedLinks();
                return links.FirstOrDefault(l => l.GetLinkDocument().Title == checkDoc.Title);
            }
        }

        /// <summary>
        /// Try to get <see cref="Transform"/> from <paramref name="checkDoc"/>.
        /// </summary>
        /// <param name="activeDoc"></param>
        /// <param name="checkDoc"></param>
        /// <returns>
        /// <see cref="Transform"/> if <paramref name="checkDoc"/> is loaded <see cref="RevitLinkInstance"/> in <paramref name="activeDoc"/> document
        /// and has <see cref="Transform"/> not equal to <see cref="Transform.Identity"/>.
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static Transform TryGetTransform(this Document activeDoc, Document checkDoc)
        {
            var link = activeDoc.TryGetLink(checkDoc);
            if (link == null) { return null; }

            var totalLinkTransform = link.GetTotalTransform();
            return totalLinkTransform.AlmostEqual(Transform.Identity) is true ?
                null : totalLinkTransform;
        }

    }
}
