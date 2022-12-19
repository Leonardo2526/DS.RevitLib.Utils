using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Collisions
{
    /// <inheritdoc/>
    public abstract class FixedCollisionLinkDetector<T, P> : ICollisionDetector
    {
        /// <summary>
        /// Current Revit document.
        /// </summary>
        protected readonly Document _doc;

        /// <summary>
        /// Fixed links elements to check collisions.
        /// </summary>
        protected readonly List<P> _checkLinkObjects;

        /// <summary>
        /// Fixed elements to exclude from collisions detection.
        /// </summary>
        protected readonly List<P> _exludedObjects;

        /// <summary>
        /// <see cref="RevitLinkInstance"/> to check its elements.
        /// </summary>
        protected readonly RevitLinkInstance _revitLinkInstance;

        /// <summary>
        /// Total <see cref="RevitLinkInstance"/> <see cref="Transform"/>.
        /// </summary>
        protected readonly Transform _linkTransform;

        /// <summary>
        /// Create a new objects for collisions (intersections) detection between objects in Revit model with fixed objects.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="checkLinkPair">Objects to detect collisions.</param>
        /// <param name="exludedElements"></param>
        protected FixedCollisionLinkDetector(Document doc, KeyValuePair<RevitLinkInstance, List<P>> checkLinkPair, List<P> exludedElements = null)
        {
            _doc = doc;
            _revitLinkInstance = checkLinkPair.Key;
            _checkLinkObjects = checkLinkPair.Value;
            _linkTransform = _revitLinkInstance.GetTotalTransform();
            _exludedObjects = exludedElements;
        }


        /// <inheritdoc/>
        public List<IBestCollision> Collisions { get; }       


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
