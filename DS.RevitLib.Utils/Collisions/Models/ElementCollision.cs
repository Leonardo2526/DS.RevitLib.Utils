using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Models;

namespace DS.RevitLib.Utils.Collisions.Models
{
    /// <inheritdoc/>
    public class ElementCollision : Collision<Element, Element>
    {
        /// <inheritdoc/>
        public ElementCollision(Element object1, Element object2) : base(object1, object2)
        {
        }
    }
}
