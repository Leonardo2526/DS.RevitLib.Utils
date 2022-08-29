using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Collisions
{
    public abstract class CollisionSearch<T, P> : ICollisionSearch
    {
        protected readonly List<T> _checkedObjects1;
        protected readonly List<P> _checkedObjects2;
        protected readonly List<P> _exludedObjects;

        protected CollisionSearch(List<T> checkedObjects1, List<P> checkedObjects2, List<P> exludedObjects=null)
        {
            _checkedObjects1 = checkedObjects1;
            _checkedObjects2 = checkedObjects2;
            _exludedObjects = exludedObjects;
        }

        protected Document Document
        {
            get
            {
                return _checkedObjects1?.First().GetDocument() ?? _checkedObjects2?.First().GetDocument();
            }
        }
        protected abstract FilteredElementCollector Collector { get; set; }
        protected abstract ExclusionFilter ExclusionFilter { get; }


        public abstract List<P> GetCollisions();

        public void SearchCollisions()
        {
            throw new NotImplementedException();
        }

        protected abstract List<P> GetObjectCollisions(T checkedObject);
    }
}
