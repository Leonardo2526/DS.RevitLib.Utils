using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.Collisions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Collisions.Checkers
{
    public class ElementCollisionChecker : CollisionChecker<Element, Element>, ICollisionChecker
    {
        public ElementCollisionChecker(List<Element> checkedObjects2, List<Element> exludedObjects = null) : 
            base(checkedObjects2, exludedObjects)
        {
        }

        //public ElementCollisionChecker(List<Element> checkedObjects1, List<Element> checkedObjects2, List<Element> exludedObjects = null) :
        //    base(checkedObjects1, checkedObjects2, exludedObjects)
        //{

        //}

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

        public List<ICollision> AllCollisions { get; private set; } = new List<ICollision>();

        protected override Document Document
        {
            get
            {
                return CheckedObjects1.First().Document;
            }
        }


        private List<ICollision> GetObjectCollisions(Element object1, List<ElementId> excludedIdsOption = null)
        {
            var excludedElementsIds = new List<ElementId>();

            if (ExludedObjects is not null && ExludedObjects.Any())
            {
                excludedElementsIds.AddRange(ExludedObjects.Select(obj => obj.Id).ToList());
            }

            excludedElementsIds.Add(object1.Id);
            var exculdedFilter = excludedIdsOption is null ? new ExclusionFilter(excludedElementsIds) : new ExclusionFilter(excludedIdsOption);

            List<Element> elements = Collector.WherePasses(new ElementIntersectsElementFilter(object1)).
                WherePasses(exculdedFilter).
                ToElements().ToList();

            //build all object1 collisions
            var collisions = new List<ICollision>();
            foreach (var elem in elements)
            {
                var col = BuildCollision(object1, elem);
                if (!CollisionExist(AllCollisions, col))
                {
                    collisions.Add(col);
                }
            }

            return collisions;
        }

        public override List<ICollision> GetCollisions()
        {
            if (Document is null || Collector is null)
            {
                throw new ArgumentNullException("Document or Collercor is null");
            }

            AllCollisions = new List<ICollision>();
            foreach (Element object1 in CheckedObjects1)
            {
                var collisions = GetObjectCollisions(object1);
                if (collisions is null || !collisions.Any())
                {
                    continue;
                }
                AllCollisions.AddRange(collisions);
            }

            return AllCollisions;
        }

        public List<ICollision> GetCollisions(List<Element> checkedObjects1, List<ElementId> excludedIdsOption = null)
        {
            CheckedObjects1 = checkedObjects1;
            AllCollisions = new List<ICollision>();

            if (CheckedObjects1 is null | CheckedObjects2 is null | 
                !CheckedObjects1.Any() | !CheckedObjects2.Any()) return AllCollisions;

            if (Document is null || Collector is null)
            {
                throw new ArgumentNullException("Document or Collercor is null");
            }

            foreach (Element object1 in CheckedObjects1)
            {
                var collisions = GetObjectCollisions(object1, excludedIdsOption);
                if (collisions is null || !collisions.Any())
                {
                    continue;
                }
                AllCollisions.AddRange(collisions);
            }

            return AllCollisions;
        }

        public override List<ICollision> GetCollisions(List<Element> checkedObjects1)
        {
            CheckedObjects1 = checkedObjects1;
            return GetCollisions();
        }

        protected override ICollision BuildCollision(Element object1, Element object2)
        {
            return new ElementTransformCollision(object1, object2);
        }

        public bool CollisionExist(List<ICollision> collisions, ICollision collision)
        {
            var sCollision = collision as ElementTransformCollision;
            foreach (ElementTransformCollision existCollison in collisions.Cast<ElementTransformCollision>())
            {
                if (sCollision.Object1.Id == existCollison.Object2.Id &&
                    existCollison.Object1.Id == sCollision.Object2.Id)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
