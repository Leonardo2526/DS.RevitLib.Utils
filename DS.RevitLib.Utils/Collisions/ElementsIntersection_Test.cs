using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Models
{
    internal class ElementsIntersection
    {
        /// <summary>
        /// Objects to check collisions.
        /// </summary>
        protected List<Element> _checkObjects2;

        /// <summary>
        /// Document of checkObjects2 used for <see cref="Autodesk.Revit.DB.FilteredElementCollector"/>;
        /// </summary>
        protected readonly Document _checkObjects2Doc;

        public ElementsIntersection(List<Element> checkObjects2, Document checkObjects2Doc)
        {
            _checkObjects2 = checkObjects2;
            _checkObjects2Doc = checkObjects2Doc;
        }

        public bool CheckValidity { get; set; } = true;

        /// <summary>
        /// Get elements in checkObjects2 that intersect <paramref name="checkSolid"/>.
        /// </summary>
        /// <param name="checkSolid"></param>
        /// <param name="exludedCheckObjects2"></param>
        /// <returns>Returns elements that intersect <paramref name="checkSolid"/>.</returns>
        public List<Element> GetIntersectedElements(Solid checkSolid, List<Element> exludedCheckObjects2 = null)
        {
            if (CheckValidity && !_checkObjects2.TrueForAll(o => o.IsValidObject))
            { _checkObjects2 = _checkObjects2.Where(o => o.IsValidObject).ToList(); }

            var collector = new FilteredElementCollector(_checkObjects2Doc, _checkObjects2.Select(el => el.Id).ToList());

            //apply quick filter.
            BoundingBoxXYZ boxXYZ = checkSolid.GetBoundingBox();
            ApplyQuickFilter(collector, boxXYZ, null);

            //apply exculsionFilter filter.
            if (exludedCheckObjects2 is not null && exludedCheckObjects2.Any())
            { collector.WherePasses(GetExclusionFilter(exludedCheckObjects2)); };

            //apply slow filter
            collector = collector.WherePasses(new ElementIntersectsSolidFilter(checkSolid));
            return collector.ToElements().ToList();
        }


        /// <summary>
        /// Get elements in checkObjects2 that intersect <paramref name="checkElement"/>.
        /// </summary>
        /// <param name="checkElement"></param>
        /// <param name="exludedCheckObjects2"></param>
        /// <returns>Returns elements that intersect <paramref name="checkElement"/>.</returns>
        public List<Element> GetIntersectedElements(Element checkElement, List<Element> exludedCheckObjects2 = null)
        {
            var doc = checkElement.Document;

            var checkObjects2Ids = new List<ElementId>();
            foreach (var item in _checkObjects2)
            {
                if (item.Id.IntegerValue == checkElement.Id.IntegerValue) { continue; }
                checkObjects2Ids.Add(item.Id);
            }
            //_checkObjects2.ForEach(o => checkObjects2Ids.Add(o.Id));

            //var fams = _checkObjects2.Where(o => o is FamilyInstance).Cast<FamilyInstance>().ToList().ForEach(f => subFamIds.AddRange(f.GetSubComponentIds()));
            var fams = _checkObjects2.Where(o => o is FamilyInstance)?.Cast<FamilyInstance>().ToList();

            foreach (var fam in fams)
            {
                var idsAdd = fam.GetSubAllElementIds();
                var subc = fam.GetSubComponentIds();
                var sub = fam.GetSubelements();
                var symb= fam.GetFamilySymbol();
                var obj = symb.GetGeometricObjects2();
                foreach (var id in idsAdd)
                {
                    var e = _checkObjects2Doc.GetElement(new ElementId(id.IntegerValue));
                    var s = e.Solid();
                }
                checkObjects2Ids.AddRange(fam.GetSubComponentIds());
            }

            var collector = new FilteredElementCollector(_checkObjects2Doc, checkObjects2Ids.ToList());
            //apply quick filter.
            BoundingBoxXYZ boxXYZ = checkElement.get_BoundingBox(null);
            var link = _checkObjects2.First().GetLink(doc);

            Transform linkTransform = null;
            if (link is not null)
            {
                var totalLinkTransform = link.GetTotalTransform();
                if (!totalLinkTransform.AlmostEqual(Transform.Identity))
                {
                    linkTransform = totalLinkTransform;
                }
            }

            //apply exculsionFilter filter.
            //exludedCheckObjects2 ??= new List<Element> { checkElement };
            if (exludedCheckObjects2 is not null && exludedCheckObjects2.Any())
            { collector.WherePasses(GetExclusionFilter(exludedCheckObjects2)); };

            ApplyQuickFilter(collector, boxXYZ, linkTransform);

            var elemes1 = collector.ToList();
            //apply slow filter
            if (linkTransform is not null)
            {
                var checkSolid = checkElement.Solid();
                checkSolid = SolidUtils.CreateTransformed(checkSolid, linkTransform.Inverse);
                return collector.WherePasses(new ElementIntersectsSolidFilter(checkSolid)).ToElements().ToList();
            }

            var checkSolid1 = checkElement.Solid();
            var se = collector.WherePasses(new ElementIntersectsSolidFilter(checkSolid1)).ToElements().ToList();

            var tee = _checkObjects2Doc.GetElement(new ElementId(2521017));
            var teeSolid = tee?.Solid();
            if (teeSolid is not null)
            {
                var interesection = BooleanOperationsUtils.ExecuteBooleanOperation(checkSolid1, teeSolid, BooleanOperationsType.Union);
            }


            //var s1 = ElementIntersectsElementFilter.IsCategorySupported(tee);
            //var s2 = ElementIntersectsElementFilter.IsElementSupported(tee);
            var filter = new ElementIntersectsElementFilter(checkElement);
            collector = collector.WherePasses(filter);
            var el = collector.ToList();

            return collector.WherePasses(new ElementIntersectsElementFilter(checkElement)).ToElements().ToList();
        }

        private void ApplyQuickFilter(FilteredElementCollector collector, BoundingBoxXYZ boxXYZ, Transform linkTransform)
        {
            var transform = boxXYZ.Transform;
            var p1 = transform.OfPoint(boxXYZ.Min);
            var p2 = transform.OfPoint(boxXYZ.Max);

            if (linkTransform != null)
            {
                p1 = linkTransform.Inverse.OfPoint(boxXYZ.Min);
                p2 = linkTransform.Inverse.OfPoint(boxXYZ.Max);
            }

            (XYZ minPoint, XYZ maxPoint) = DS.RevitLib.Utils.XYZUtils.CreateMinMaxPoints(new List<XYZ> { p1, p2 });


            var outline = new Outline(minPoint, maxPoint);
            collector.WherePasses(new BoundingBoxIntersectsFilter(outline, 0));
        }


        private ExclusionFilter GetExclusionFilter(List<Element> excludedElements)
        {
            var excludedElementsIds = excludedElements?.Select(el => el.Id).ToList();
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
    }
}
