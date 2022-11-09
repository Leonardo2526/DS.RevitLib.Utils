using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public List<ICollision> AllCollisions { get; private set; } = new List<ICollision>();
        protected override Document Document
        {
            get
            {
                return CheckedObjects1.First().Element.Document;
            }
        }


        private List<ICollision> GetObjectCollisions(SolidModelExt object1, List<ElementId> excludedIdsOption = null)
        {
            var excludedElementsIds = new List<ElementId>();

            if (ExludedObjects is not null && ExludedObjects.Any())
            {
                excludedElementsIds.AddRange(ExludedObjects.Select(obj => obj.Id).ToList());
            }

            excludedElementsIds.Add(object1.Element.Id);
            var exculdedFilter = excludedIdsOption is null ? new ExclusionFilter(excludedElementsIds) : new ExclusionFilter(excludedIdsOption);

            List<Element> elements = Collector.WherePasses(new ElementIntersectsSolidFilter(object1.Solid)).
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
            foreach (SolidModelExt object1 in CheckedObjects1)
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

        public List<ICollision> GetCollisions(List<SolidModelExt> checkedObjects1, List<ElementId> excludedIdsOption = null)
        {
            CheckedObjects1 = checkedObjects1;

            if (Document is null || Collector is null)
            {
                throw new ArgumentNullException("Document or Collercor is null");
            }

            AllCollisions = new List<ICollision>();
            foreach (SolidModelExt object1 in CheckedObjects1)
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

        public override List<ICollision> GetCollisions(List<SolidModelExt> checkedObjects1)
        {
            CheckedObjects1 = checkedObjects1;
            return GetCollisions();
        }

        protected override ICollision BuildCollision(SolidModelExt object1, Element object2)
        {
            return new SolidElemCollision(object1, object2);
        }

        public bool CollisionExist(List<ICollision> collisions, ICollision collision)
        {
            var sCollision = collision as SolidElemCollision;
            foreach (SolidElemCollision existCollison in collisions.Cast<SolidElemCollision>())
            {
                if (sCollision.Object1.Element.Id == existCollison.Object2.Id &&
                    existCollison.Object1.Element.Id == sCollision.Object2.Id)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
