using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Collisions
{
    /// <inheritdoc/>
    public abstract class StaticCollisionDetector<T, P> : CollisionDetector
    {
        /// <summary>
        /// Fixed elements to check collisions.
        /// </summary>
        protected readonly List<P> _checkObjects2;

        /// <summary>
        /// Fixed elements to exclude from collisions detection.
        /// </summary>
        protected readonly List<P> _checkObjects2ToExclude;

        /// <summary>
        /// Document of checkObjects2 used for <see cref="Autodesk.Revit.DB.FilteredElementCollector"/>;
        /// </summary>
        protected readonly Document _checkObjects2Doc;

        /// <inheritdoc/>
        /// <param name="doc"></param>
        /// <param name="checkObjects2">Objects to detect collisions.</param>
        /// <param name="checkObjects2ToExclude"></param>
        protected StaticCollisionDetector(Document doc, List<P> checkObjects2, List<P> checkObjects2ToExclude = null) : base(doc)
        { 
            _checkObjects2 = checkObjects2;
            _checkObjects2ToExclude = checkObjects2ToExclude;
            _checkObjects2Doc = doc;
        }


        /// <summary>
        /// Create a new objects for collisions (intersections) detection between objects in Revit model with link objects.
        /// </summary>
        /// <param name="revitLink"></param>
        /// <param name="checkLinkObjects">Objects to detect collisions.</param>
        /// <param name="exludedElementsInLink"></param>
        protected StaticCollisionDetector(RevitLinkInstance revitLink, List<P> checkLinkObjects, List<P> exludedElementsInLink = null) : base(revitLink)
        {
            _checkObjects2 = checkLinkObjects;
            _checkObjects2ToExclude = exludedElementsInLink;
            _checkObjects2Doc = _linkDoc;
        }

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
