using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Collisions
{
    /// <inheritdoc/>
    public abstract class CollisionDetector<T,P>: ICollisionDetector
    {
        /// <summary>
        /// Current Revit document.
        /// </summary>
        protected readonly Document _doc;

        /// <summary>
        /// <see cref="RevitLinkInstance"/> document.
        /// </summary>
        protected readonly Document _linkDoc;

        /// <summary>
        /// <see cref="RevitLinkInstance"/> to check its elements.
        /// </summary>
        protected readonly RevitLinkInstance _revitLink;

        /// <summary>
        /// Objects to check collisions.
        /// </summary>
        protected readonly List<P> _checkObjects2;

        /// <summary>
        /// Document of checkObjects2 used for <see cref="Autodesk.Revit.DB.FilteredElementCollector"/>;
        /// </summary>
        protected readonly Document _checkObjects2Doc;


        /// <summary>
        /// Create a new object for collisions (intersections) detection with objects in <see cref="Document"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="checkObjects2">Objects to detect collisions.</param>
        protected CollisionDetector(Document doc, List<P> checkObjects2)
        {
            _doc = doc;
            _checkObjects2 = checkObjects2;
            _checkObjects2Doc = doc;
        }

        /// <summary>
        /// Create a new objects for collisions (intersections) detection between objects in Revit model with link objects.
        /// </summary>
        /// <param name="revitLink"></param>
        /// <param name="checkLinkObjects">Objects to detect collisions.</param>
        protected CollisionDetector(RevitLinkInstance revitLink, List<P> checkLinkObjects) 
        {
            _doc = revitLink.Document;
            _linkDoc = revitLink.GetLinkDocument();
            _revitLink = revitLink;
            _checkObjects2 = checkLinkObjects;
            _checkObjects2Doc = _linkDoc;
        }


        /// <inheritdoc/>
        public List<IBestCollision> Collisions { get; }

        /// <summary>
        /// Get collisions between <paramref name="checkObjects1"/> and checkObjects2/>.
        /// </summary>
        /// <param name="checkObjects1"></param>
        /// <param name="checkObjects2ToExclude"></param>
        /// <returns>Return <see cref="IBestCollision"/> if it exitst. Returns null if collision wasn't found between objects.</returns>
        public abstract List<IBestCollision> GetCollisions(T checkObjects1, List<P> checkObjects2ToExclude = null);

    }
}
