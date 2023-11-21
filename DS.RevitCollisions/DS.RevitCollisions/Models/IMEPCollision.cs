using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;

namespace DS.RevitCollisions.Models
{
    /// <inheritdoc/>
    public interface IMEPCollision : IStatusCollision<MEPCurve, Element>
    { }

}