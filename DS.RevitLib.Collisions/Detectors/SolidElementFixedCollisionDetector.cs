using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Collisions
{
    /// <inheritdoc/>
    public class SolidElementFixedCollisionDetector : FixedCollisionDetector<Solid, Element>
    {
        /// <inheritdoc/>
        public SolidElementFixedCollisionDetector(Document doc, List<Element> checkObjects2, List<Element> exludedElements = null) : 
            base(doc, checkObjects2, exludedElements)
        {
        }

       /// <inheritdoc/>
        protected override FilteredElementCollector Collector
        {
            get
            {
                ICollection<ElementId> checkedObjects2Ids = _checkObjects2?.Select(el => el.Id).ToList();
                return checkedObjects2Ids is null ? null : new FilteredElementCollector(_doc, checkedObjects2Ids);
            }
        }

        /// <inheritdoc/>
        public override List<IBestCollision> GetCollision(Solid checkObject1)
        {
            var excludedElementsIds = _exludedObjects is null || !_exludedObjects.Any() ? 
                new List<ElementId>() : 
                _exludedObjects.Select(el => el.Id).ToList();
            var exculdedFilter = new ExclusionFilter(excludedElementsIds);

            List<Element> elements = Collector.
                WherePasses(new ElementIntersectsSolidFilter(checkObject1)).
                WherePasses(exculdedFilter).
                ToElements().ToList();

            var collisions = new List<IBestCollision>();
            elements.ForEach(obj => collisions.Add(new SolidElementCollision(checkObject1, obj)));

            return collisions;
        }

        /// <inheritdoc/>
        public override List<IBestCollision> GetCollisions(List<Solid> checkObjects1)
        {
            var collisions = new List<IBestCollision>();
            checkObjects1.ForEach(obj => collisions.AddRange(GetCollision(obj)));
            return collisions;
        }
    }
}
