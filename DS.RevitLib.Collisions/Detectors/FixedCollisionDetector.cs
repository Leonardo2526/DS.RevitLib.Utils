using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Collisions
{
    /// <inheritdoc/>
    public abstract class FixedCollisionDetector<T, P> : ICollisionDetector
    {
        /// <summary>
        /// Current Revit document.
        /// </summary>
        protected readonly Document _doc;

        /// <summary>
        /// Fixed elements to check collisions.
        /// </summary>
        protected readonly List<P> _checkObjects2;

        /// <summary>
        /// Fixed elements to exclude from collisions detection.
        /// </summary>
        protected readonly List<P> _exludedObjects;


        /// <summary>
        /// Create a new objects for collisions (intersections) detection between objects in Revit model with fixed objects.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="checkObjects2">Objects to detect collisions.</param>
        /// <param name="exludedElements"></param>
        protected FixedCollisionDetector(Document doc, List<P> checkObjects2, List<P> exludedElements = null)
        {
            _doc = doc;
            _checkObjects2 = checkObjects2;
            _exludedObjects = exludedElements;
        }


        /// <inheritdoc/>
        public List<IBestCollision> Collisions { get; }

        /// <summary>
        /// <see cref="Autodesk.Revit.DB.FilteredElementCollector"/> to detect collisions between checkedObjects1 and checkedObjects2.
        /// </summary>
        protected abstract FilteredElementCollector Collector { get; }


        /// <summary>
        /// Get collisions between <paramref name="checkObjects1"/> and checkObjects2/>.
        /// </summary>
        /// <param name="checkObjects1"></param>
        /// <returns>Returns <see cref="IBestCollision"/> list of detected collisions. Returns empty list if no collisions was detected .</returns>
        public abstract List<IBestCollision> GetCollisions(List<T> checkObjects1);

        /// <summary>
        /// Get collisions between <paramref name="checkObjects1"/> and checkObjects2/>.
        /// </summary>
        /// <param name="checkObjects1"></param>
        /// <returns>Return <see cref="IBestCollision"/> if it exitst. Returns null if collision wasn't found between objects.</returns>
        public abstract List<IBestCollision> GetCollision(T checkObjects1);
    }
}
