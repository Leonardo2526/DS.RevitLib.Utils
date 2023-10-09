using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using DS.RevitLib.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Collisions.Models
{
    /// <summary>
    /// An object that represents find intersection between <see cref="Autodesk.Revit.DB.Element"/>'s.
    /// </summary>
    public class BestElementIntersectionFactory : IElementIntersectionFactory
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
        public BestElementIntersectionFactory(Document activeDocument)
        {
            _activeDocument = activeDocument;
        }

        /// <inheritdoc/>
        public List<Element> ExludedElements { get; set; }

        /// <inheritdoc/>
        public List<Element> Intersections { get; private set; }

        /// <inheritdoc/>
        public IElementIntersectionFactory Build((Document, List<Element>) checkModel2)
        {
            _checkModel2 = checkModel2;
            _linkTransform = _activeDocument.TryGetTransform(checkModel2.Item1);
            return this;
        }

        /// <inheritdoc/>
        public List<Element> GetIntersections(Element checkObject) => GetObjectIntersections(checkObject);

        /// <inheritdoc/>
        public List<Element> GetIntersections(Solid checkObject) => GetObjectIntersections(checkObject);


        #region PrivateMethods

        private List<Element> GetObjectIntersections(object checkObject)
        {
            _collector = new FilteredElementCollector(_checkModel2.Item1, _checkModel2.Item2.Select(el => el.Id).ToList());

            _quickFilters.Clear();
            _slowFilters.Clear();

            _quickFilters.Add(GetBoundingBoxIntersectsFilter(checkObject));
            var exclusionFilter = GetExclusionFilter(ExludedElements);
            if (exclusionFilter != null) { _quickFilters.Add(exclusionFilter); }
            _quickFilters.ForEach(filter => _collector.WherePasses(filter));
            var elems2 = _collector.ToElements().ToList();

            _slowFilters.Add(GetElementIntersectsFilter(checkObject));

            _slowFilters.ForEach(filter => _collector.WherePasses(filter));

            return Intersections = _collector.ToElements().ToList();
        }

        private ElementQuickFilter GetBoundingBoxIntersectsFilter(object checkObject)
        {
            BoundingBoxXYZ boxXYZ = GetBoxXYZ(checkObject);

            var transform = boxXYZ.Transform;
            var p1 = transform.OfPoint(boxXYZ.Min);
            var p2 = transform.OfPoint(boxXYZ.Max);

            if (_linkTransform != null)
            {
                p1 = _linkTransform.Inverse.OfPoint(boxXYZ.Min);
                p2 = _linkTransform.Inverse.OfPoint(boxXYZ.Max);
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
            var excludedElementsInsulationIds = new List<ElementId>();
            excludedElements.ForEach(obj =>
            {
                if (obj is Pipe || obj is Duct)
                {
                    Document doc = obj.Document;
                    Element insulation = InsulationLiningBase.GetInsulationIds(doc, obj.Id)?
                      .Select(x => doc.GetElement(x)).FirstOrDefault();
                    if (insulation != null) { excludedElementsInsulationIds.Add(insulation.Id); }
                }
            });
            excludedElementsIds.AddRange(excludedElementsInsulationIds);

            return excludedElementsIds is null || !excludedElementsIds.Any() ?
                 null : new ExclusionFilter(excludedElementsIds);
        }

        #endregion


    }
}
