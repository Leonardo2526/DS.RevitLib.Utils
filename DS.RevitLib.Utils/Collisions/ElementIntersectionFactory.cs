using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Collisions.Models
{
    /// <summary>
    /// An object that represents find intersection between <see cref="Autodesk.Revit.DB.Element"/>'s.
    /// </summary>
    public class ElementIntersectionFactory : IElementIntersectionFactory
    {
        private readonly Document _activeDocument;
        private Transform _linkTransform;
        private (Document, List<Element>) _checkModel2;
        private FilteredElementCollector _collector;
        private readonly List<ElementQuickFilter> _quickFilters = new();
        private readonly List<ElementSlowFilter> _slowFilters = new();

        /// <summary>
        /// Instantiate an object to find intersection between <see cref="Autodesk.Revit.DB.Element"/>'s.
        /// </summary>
        /// <param name="activeDocument"></param>
        public ElementIntersectionFactory(Document activeDocument)
        {
            _activeDocument = activeDocument;
        }


        /// <summary>
        /// Types to exclude from intersections.
        /// </summary>
        public List<BuiltInCategory> ExculdedCategories { get; set; } = new List<BuiltInCategory>();

        /// <summary>
        /// Types to exclude from intersections.
        /// </summary>
        public List<Type> ExculdedTypes { get; set; } = new List<Type>();

        /// <inheritdoc/>
        public List<Element> ExcludedElements { get; set; } = new List<Element>();

        /// <inheritdoc/>
        public List<Element> Intersections { get; private set; }

        /// <inheritdoc/>
        public bool IsInsulationAccount => !ExculdedTypes.Contains(typeof(InsulationLiningBase));

        private List<ElementId> _ExcludedCategoriesIds
        {
            get
            {
                if (ExculdedCategories is null || !ExculdedCategories.Any()) { return new List<ElementId>(); }

                var excludedCategoriesIds = new List<ElementId>();
                ExculdedCategories.ForEach(c => excludedCategoriesIds.Add(new ElementId((int)c)));
                return excludedCategoriesIds;
            }
        }

        /// <inheritdoc/>
        public IElementIntersectionFactory Build((Document, List<Element>) checkModel2)
        {
            _checkModel2 = checkModel2;
            _linkTransform = null;
            return this;
        }

        /// <inheritdoc/>
        public IElementIntersectionFactory Build((RevitLinkInstance, Transform, List<Element>) checkModel2)
        {
            var link = checkModel2.Item1;
            _checkModel2 = (link.GetLinkDocument(), checkModel2.Item3);
            _linkTransform = checkModel2.Item2;
            return this;
        }

        /// <inheritdoc/>
        public List<Element> GetIntersections(Element checkObject) => GetObjectIntersections(checkObject);

        /// <inheritdoc/>
        public List<Element> GetIntersections(Solid checkObject) => GetObjectIntersections(checkObject);


        #region PrivateMethods

        private List<Element> GetObjectIntersections(object checkObject)
        {
            if (_checkModel2.Item2 is not null && _checkModel2.Item2.Count == 0)
            { return new List<Element>(); }

            _collector = _checkModel2.Item2 is null ?
                new FilteredElementCollector(_checkModel2.Item1) :
                new FilteredElementCollector(_checkModel2.Item1, _checkModel2.Item2.Select(el => el.Id).ToList());

            _quickFilters.Clear();
            _slowFilters.Clear();

            _quickFilters.Add(GetBoundingBoxIntersectsFilter(checkObject));

            //set exclusionFilter
            var exclusionFilter = GetExclusionFilter(ExcludedElements);
            if (exclusionFilter != null) { _quickFilters.Add(exclusionFilter); }

            //set multiClassFilter
            var multiClassFilter = !ExculdedTypes.Any() ?
                null : new ElementMulticlassFilter(ExculdedTypes, true);
            if (multiClassFilter != null) { _quickFilters.Add(multiClassFilter); }

            //set multicategoryFilter
            var multiCategoryFilter = !_ExcludedCategoriesIds.Any() ?
                null : new ElementMulticategoryFilter(_ExcludedCategoriesIds, true);
            if (multiCategoryFilter != null) { _quickFilters.Add(multiCategoryFilter); }

            //apply quick filters
            _quickFilters.ForEach(filter => _collector.WherePasses(filter));

            _collector.WhereElementIsNotElementType();

            _slowFilters.Add(GetElementIntersectsFilter(checkObject));
            _slowFilters.ForEach(filter => _collector.WherePasses(filter));

            var elements = _collector.ToElements();
            return Intersections = elements.Where(e => e.IsGeometryElement()).ToList();
        }

        private ElementQuickFilter GetBoundingBoxIntersectsFilter(object checkObject)
        {
            BoundingBoxXYZ boxXYZ = GetBoxXYZ(checkObject);

            var transform = boxXYZ.Transform;
            var p1 = transform.OfPoint(boxXYZ.Min);
            var p2 = transform.OfPoint(boxXYZ.Max);

            if (_linkTransform != null)
            {
                p1 = _linkTransform.Inverse.OfPoint(p1);
                p2 = _linkTransform.Inverse.OfPoint(p2);
            }

            (XYZ minPoint, XYZ maxPoint) = DS.RevitLib.Utils.XYZUtils.CreateMinMaxPoints(new List<XYZ> { p1, p2 });
            var outline = new Outline(minPoint, maxPoint);

            return new BoundingBoxIntersectsFilter(outline, 0);
        }

        private BoundingBoxXYZ GetBoxXYZ(object checkObject)
        {
            BoundingBoxXYZ boxXYZ = null;
            if (checkObject is Element element)
            { boxXYZ = element.get_BoundingBox(null); }
            else if (checkObject is Solid solid)
            { boxXYZ = solid.GetBoundingBox(); }

            return boxXYZ;
        }

        private ElementSlowFilter GetElementIntersectsFilter(object checkObject)
        {
            ElementSlowFilter filter = null;
            if (checkObject is Element element)
            {
                filter = _linkTransform is null ?
                    new ElementIntersectsElementFilter(element) :
                    null;
                filter ??= GetSolidFilter(element.Solid());
            }
            else if (checkObject is Solid solid)
            { filter = GetSolidFilter(solid); }

            return filter;


            ElementSlowFilter GetSolidFilter(Solid checkObject)
            {
                var checkSolid = _linkTransform is null ?
                    checkObject :
                    SolidUtils.CreateTransformed(checkObject, _linkTransform.Inverse);
                return new ElementIntersectsSolidFilter(checkSolid);
            }
        }

        private ExclusionFilter GetExclusionFilter(List<Element> excludedElements)
        {
            if (excludedElements is null || excludedElements.Count == 0) { return null; }

            var excludedElementsIds = excludedElements.Select(el => el.Id).ToList();

            if (IsInsulationAccount)
            {
                var excludedElementsInsulationIds = new List<ElementId>();
                excludedElements.ForEach(obj =>
                {
                    if (obj is Pipe || obj is Duct)
                    {
                        Element insulation = obj.GetInsulation();
                        if (insulation != null) { excludedElementsInsulationIds.Add(insulation.Id); }
                    }
                });
                excludedElementsIds.AddRange(excludedElementsInsulationIds);
            }

            return excludedElementsIds is null || !excludedElementsIds.Any() ?
                 null : new ExclusionFilter(excludedElementsIds);
        }


        #endregion


    }
}
