using Autodesk.Revit.DB;

namespace DS.RevitLib.Collisions2
{
    /// <inheritdoc/>
    public class SolidElementCollision : BestCollision<Solid, Element>
    {
        /// <inheritdoc/>
        public SolidElementCollision(Solid object1, Element object2) : base(object1, object2)
        {
        }
    }
}
