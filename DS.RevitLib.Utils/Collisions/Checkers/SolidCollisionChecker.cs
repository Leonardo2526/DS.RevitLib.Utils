using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Checkers
{
    public class SolidCollisionChecker : CollisionChecker<SolidModelExt, Element>, ICollisionChecker
    {
        public SolidCollisionChecker(List<Element> checkedObjects2, List<Element> exludedObjects = null) :
            base(checkedObjects2, exludedObjects)
        {
        }

        public SolidCollisionChecker(List<SolidModelExt> checkedObjects1, List<Element> checkedObjects2, List<Element> exludedObjects = null) : 
            base(checkedObjects1, checkedObjects2, exludedObjects)
        {
        }

        protected override FilteredElementCollector Collector
        {
            get
            {
                ICollection<ElementId> checkedObjects2Ids = CheckedObjects2?.Select(el => el.Id).ToList();
                return checkedObjects2Ids is null ? null : new FilteredElementCollector(Document, checkedObjects2Ids);
            }
            set { }
        }

        protected override ExclusionFilter ExclusionFilter
        {
            get
            {
                ICollection<ElementId> excludedElementsIds = ExludedObjects?.Select(el => el.Id).ToList();
                return excludedElementsIds is null ? null : new ExclusionFilter(excludedElementsIds);
            }
        }


        private List<ICollision> GetObjectCollisions(SolidModelExt object1)
        {
            List<Element> elements;

            if (ExclusionFilter is null)
            {
                elements = Collector.WherePasses(new ElementIntersectsSolidFilter(object1.Solid)).
                    ToElements().ToList();
            }
            else
            {
                elements = Collector.WherePasses(new ElementIntersectsSolidFilter(object1.Solid)).
                    WherePasses(ExclusionFilter).
                    ToElements().ToList();
            }

            //build all object1 collisions
            var collisions = new List<ICollision>();
            foreach (var elem in elements)
            {
                var col = BuildCollision(object1, elem);
                collisions.Add(col);
            }

            return collisions;
        }

        public override List<ICollision> GetCollisions()
        {
            if (Document is null || Collector is null)
            {
                return null;
            }

            var allCollisions = new List<ICollision>();

            foreach (var item in CheckedObjects1)
            {
                var collisions = GetObjectCollisions(item);
                if (collisions is null || !collisions.Any())
                {
                    continue;
                }
                allCollisions.AddRange(collisions);
            }

            return allCollisions;
        }

        protected override ICollision BuildCollision(SolidModelExt object1, Element object2)
        {
            return new SolidElemCollision(object1, object2);
        }

        public override List<ICollision> GetCollisions(List<SolidModelExt> checkedObjects1)
        {
            CheckedObjects1 = checkedObjects1;
            return GetCollisions();

        }
    }
}
