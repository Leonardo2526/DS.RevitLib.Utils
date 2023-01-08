using Autodesk.Revit.DB;

namespace DS.RevitLib.Collisions2
{
    /// <inheritdoc/>
    public class ElementCollision : BestCollision<Element, Element>
    {
        /// <inheritdoc/>
        public ElementCollision(Element object1, Element object2) : base(object1, object2)
        {
        }
    }
}
