using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Collisions
{
    /// <summary>
    /// An object that represents utils to work with <see cref="BoundingBoxXYZ"/> collisions of elements.
    /// </summary>
    public class BBCollisionUtils
    {
        private readonly Document _doc;
        private readonly List<Element> _elements;
        private readonly List<RevitLinkInstance> _allLinks;

        /// <summary>
        /// Instantiate an object to find collisions by <see cref="BoundingBoxXYZ"/>.
        /// </summary>
        public BBCollisionUtils(Document doc, List<Element> elements, List<RevitLinkInstance> allLinks)
        {
            _doc = doc;
            _elements = elements;
            _allLinks = allLinks;
        }

        /// <summary>
        /// All elements collector in current <see cref="Document"/>.
        /// </summary>
        public FilteredElementCollector Collector
        {
            get
            {
                ICollection<ElementId> checkedObjects2Ids = _elements?.Select(el => el.Id).ToList();
                return checkedObjects2Ids is null ? null : new FilteredElementCollector(_doc, checkedObjects2Ids);
            }
            set { }
        }

        /// <summary>
        /// Get all elements which have a bounding box which intersects a given <paramref name="outline"/>.
        /// </summary>
        /// <param name="outline"></param>
        /// <param name="tolerance">  
        /// If the tolerance is positive, the outlines may be separated by the tolerance
        /// distance in each coordinate. If the tolerance is negative, the outlines must
        /// overlap by at least the tolerance distance in each coordinate.</param>
        /// <param name="exludedObjects"></param>
        /// <returns>Returns all intersected elements in current <see cref="Document"/> and all objects of <see cref="RevitLinkInstance"/> in it.</returns>
        public List<Element> GetElements(Outline outline, double tolerance = 0, List<Element> exludedObjects = null)
        {
            ExclusionFilter exclusionFilter = GetExclusionFilter(exludedObjects);

            List<Element> allElementsInOutline = new List<Element>();

            //get elements by outline in current Document
            List<Element> docElements = Collector?.WherePasses(new BoundingBoxIntersectsFilter(outline, tolerance)).
                WherePasses(exclusionFilter).
                   ToElements().ToList();
            if(docElements!= null && docElements.Any()) { allElementsInOutline.AddRange(docElements); }
            else { return allElementsInOutline; }
            


            //get elements by outline in all links
            ICollection<ElementId> checkedObjects2Ids = _elements?.Select(el => el.Id).ToList();
            foreach (var link in _allLinks)
            {
                var linkCollector = checkedObjects2Ids is null ? null :
                    new FilteredElementCollector(link.GetLinkDocument(), checkedObjects2Ids);

                var tr = link.GetTransform();
                var linkOutLine = new Outline(tr.Inverse.OfPoint(outline.MinimumPoint), tr.Inverse.OfPoint(outline.MaximumPoint));

                //show outlint points
                //linkOutLine.MinimumPoint.Show(_doc);
                //linkOutLine.MaximumPoint.Show(_doc);

                List<Element> linkElements = linkCollector?.
                    WherePasses(new BoundingBoxIntersectsFilter(linkOutLine, tolerance)).
                    WherePasses(exclusionFilter).ToElements().ToList();

                allElementsInOutline.AddRange(linkElements);
            }


            return allElementsInOutline;
        }

        private ExclusionFilter GetExclusionFilter(List<Element> exludedObjects)
        {
            var excludedElementsIds = new List<ElementId>();

            if (exludedObjects is not null && exludedObjects.Any())
            {
                excludedElementsIds.AddRange(exludedObjects.Select(obj => obj.Id).ToList());
            }

            return new ExclusionFilter(excludedElementsIds);
        }
    }
}
