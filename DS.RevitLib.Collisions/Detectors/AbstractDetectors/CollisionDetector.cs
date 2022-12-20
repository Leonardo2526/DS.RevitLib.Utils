using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Collisions
{
    /// <inheritdoc/>
    public abstract class CollisionDetector: ICollisionDetector
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
        /// Create a new object for collisions (intersections) detection with objects in <see cref="Document"/>.
        /// </summary>
        /// <param name="doc"></param>
        protected CollisionDetector(Document doc)
        {
            _doc = doc;
        }

        /// <summary>
        /// Create a new object for collisions (intersections) detection with objects in <see cref="RevitLinkInstance"/>.
        /// </summary>
        /// <param name="revitLink"></param>
        protected CollisionDetector(RevitLinkInstance revitLink)
        {
            _doc = revitLink.Document;
            _linkDoc = revitLink.GetLinkDocument();
            _revitLink = revitLink;
        }

        /// <inheritdoc/>
        public List<IBestCollision> Collisions { get; }
    }
}
