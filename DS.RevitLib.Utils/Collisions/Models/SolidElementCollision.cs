using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.Collisions.Models
{
    /// <inheritdoc/>
    public class SolidElementCollision : Collision<Solid, Element>
    {
        /// <inheritdoc/>
        public SolidElementCollision(Solid object1, Element object2) : base(object1, object2)
        {
        }
    }
}
