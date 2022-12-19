using Autodesk.Revit.DB;

namespace DS.RevitLib.Collisions
{
    /// <inheritdoc/>
    public class SolidElementCollision : BestCollision<Solid, Element>
    {
        /// <inheritdoc/>
        public SolidElementCollision(Solid object1, Element object2) : base(object1, object2)
        {
        }

        public Transform Transform1 { get; set; }
        public Transform Transform2 { get; set; }
    }
}
