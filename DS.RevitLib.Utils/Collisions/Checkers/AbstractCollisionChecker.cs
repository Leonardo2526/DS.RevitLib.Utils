using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Checkers
{
    public abstract class AbstractCollisionChecker<T, P> : ICollisionChecker
    {
        protected readonly List<T> _checkedObjects1;
        protected readonly List<P> _checkedObjects2;
        protected readonly List<P> _exludedObjects;

        protected AbstractCollisionChecker(List<T> checkedObjects1, List<P> checkedObjects2, List<P> exludedObjects = null)
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

        protected abstract List<ICollision> GetCollisions(T object1);
        public abstract List<ICollision> GetCollisions();

        protected abstract List<ICollision> BuildCollisions(T object1, List<P> objects2);
    }
}
