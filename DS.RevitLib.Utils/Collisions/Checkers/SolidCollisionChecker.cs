using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Checkers
{
    public class SolidCollisionChecker : AbstractCollisionChecker<Solid, Element>, ICollisionChecker
    {
        public SolidCollisionChecker(List<Solid> checkedObjects1, List<Element> checkedObjects2, List<Element> exludedObjects = null) :
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


        protected override List<ICollision> GetCollisions(Solid object1)
        {
            List<Element> elements;

            if (ExclusionFilter is null)
            {
                elements = Collector.WherePasses(new ElementIntersectsSolidFilter(object1)).
                    ToElements().ToList();
            }
            else
            {
                elements = Collector.WherePasses(new ElementIntersectsSolidFilter(object1)).
                    WherePasses(ExclusionFilter).
                    ToElements().ToList();
            }

            return BuildCollisions(object1, elements);
        }

        public override List<ICollision> GetCollisions()
        {
            if (Document is null || Collector is null)
            {
                return null;
            }

            var allCollisions = new List<ICollision>();

            foreach (var item in _checkedObjects1)
            {
                var collisions = GetCollisions(item);
                if (collisions is null || !collisions.Any())
                {
                    continue;
                }
                allCollisions.AddRange(collisions);
            }

            return allCollisions;
        }

        protected override List<ICollision> BuildCollisions(Solid object1, List<Element> objects2)
        {
            var collisions = new List<ICollision>();
            var collision = new SolidElemCollision(object1, objects2);
            collisions.Add(collision);

            return collisions;
        }
    }
}
