using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.Revit.Utils
{
    public class SolidByCollector
    {
        private FilteredElementCollector Collector;
        private readonly BoundingBoxIntersectsFilter _boundingBoxFilter;
        private readonly ExclusionFilter _exclusionFilter;

        //Get solids by collector and filters. Filters are optional.
        public SolidByCollector(FilteredElementCollector collector,
            BoundingBoxIntersectsFilter boundingBoxFilter = null,
            ExclusionFilter exclusionFilter = null)
        {
            Collector = collector;
            _boundingBoxFilter = boundingBoxFilter;
            _exclusionFilter = exclusionFilter;
        }


        /// <summary>
        /// Get solids of elements by boundig box from current model.
        /// </summary>   
        public Dictionary<Element, List<Solid>> GetModelSolids()
        {
            IList<Element> elements = GetElements();

            Dictionary<Element, List<Solid>> collectorSolids = new Dictionary<Element, List<Solid>>();

            foreach (Element elem in elements)
            {
                List<Solid> solids = ElementUtils.GetSolids(elem);
                collectorSolids.Add(elem, solids);
            }

            return collectorSolids;
        }

        /// <summary>
        /// Get elements for solids search
        /// </summary>
        /// <returns></returns>
        private IList<Element> GetElements()
        {
            //Get elements by bounding box
            if (_boundingBoxFilter != null)
            {
                Collector.WherePasses(_boundingBoxFilter);
            }

            //Get elements by exclusions
            if (_exclusionFilter != null)
            {
                Collector.WherePasses(_exclusionFilter);
            }

            return Collector.ToElements();
        }
    }
}
