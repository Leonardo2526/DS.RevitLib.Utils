using Autodesk.Revit.Creation;
using System.Collections.Generic;

namespace DS.RevitLib.Collisions
{
    /// <inheritdoc/>
    public abstract class CollisionDetector<T, P> : ICollisionDetector
    {
        /// <summary>
        /// Current Revit document.
        /// </summary>
        protected readonly Document _doc;

        /// <summary>
        /// Create a new objects for collisions (intersections) detection between objects.
        /// </summary>
        /// <param name="doc"></param>
        protected CollisionDetector(Document doc)
        {
            _doc = doc;
        }


        /// <inheritdoc/>
        public List<IBestCollision> Collisions { get; }

        /// <summary>
        /// Get collisions between <paramref name="checkObjects1"/> and <paramref name="checkObjects2"/>.
        /// </summary>
        /// <param name="checkObjects1"></param>
        /// <param name="checkObjects2"></param>
        /// <returns>Returns <see cref="IBestCollision"/> list of detected collisions. Returns empty list if no collisions was detected .</returns>
        public abstract List<IBestCollision> GetCollisions(List<T> checkObjects1, List<P> checkObjects2);

        /// <summary>
        /// Get collisions between <paramref name="checkObject1"/> and <paramref name="checkObject2"/>.
        /// </summary>
        /// <param name="checkObject1"></param>
        /// <param name="checkObject2"></param>
        /// <returns>Return <see cref="IBestCollision"/> if it exitst. Returns null if collision wasn't found between objects.</returns>
        public abstract List<IBestCollision> GetCollision(T checkObject1, P checkObject2);
    }
}
