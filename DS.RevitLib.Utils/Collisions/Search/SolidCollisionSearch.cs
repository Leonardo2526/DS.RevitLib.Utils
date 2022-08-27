using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Collisions
{
    public class SolidCollisionSearch : CollisionSearch<Solid, Element>
    {
        public SolidCollisionSearch(List<Solid> checkedObjects1, List<Element> checkedObjects2, 
            List<Element> exludedObjects = null) :
            base(checkedObjects1, checkedObjects2, exludedObjects)
        {
        }

        protected override FilteredElementCollector Collector
        {
            get
            {
                ICollection<ElementId> checkedObjects2Ids = _checkedObjects2?.Select(el => el.Id).ToList();
                return checkedObjects2Ids is null ? null : new FilteredElementCollector(Document, checkedObjects2Ids);
            }
            set { }
        }

        protected override ExclusionFilter ExclusionFilter
        {
            get
            {
                ICollection<ElementId> excludedElementsIds = _exludedObjects?.Select(el => el.Id).ToList();
                return excludedElementsIds is null ? null : new ExclusionFilter(excludedElementsIds);
            }
        }


        public override List<Element> GetCollisions()
        {
            if (Document is null || Collector is null)
            {
                return null;
            }
                       
            var list = new List<Element>();

            foreach (var item in _checkedObjects1)
            {
                var collisions = GetObjectCollisions(item);
                if (collisions is null || !collisions.Any())
                {
                    continue;
                }
                list.AddRange(collisions);
            }

            return list;
        }

        protected override List<Element> GetObjectCollisions(Solid checkedObject)
        {
            if (ExclusionFilter is null)
            {
                return Collector.WherePasses(new ElementIntersectsSolidFilter(checkedObject)).
                    ToElements().ToList();
            }
            else
            {
                return Collector.WherePasses(new ElementIntersectsSolidFilter(checkedObject)).
                    WherePasses(ExclusionFilter).
                    ToElements().ToList();
            }           
        }
    }
}
