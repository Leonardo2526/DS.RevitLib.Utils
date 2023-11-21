using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using System.Threading.Tasks;

namespace DS.RevitCollisions.Models
{
    /// <inheritdoc/>
    public interface IElementCollision : IStatusCollision<Element, Element>
    {
       
    }
}
