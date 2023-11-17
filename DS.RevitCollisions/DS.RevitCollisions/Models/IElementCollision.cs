using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using System.Threading.Tasks;

namespace DS.RevitCollisions.Models
{
    /// <summary>
    /// Represents intersection between two <see cref="Autodesk.Revit.DB.Element"/>'s.
    /// </summary>
    public interface IElementCollision : ICollision
    {
        /// <summary>
        /// Current collision staus.
        /// </summary>
        CollisionStatus Status { get; }

        /// <summary>
        /// Element with changeable position.
        /// </summary>
        Element ResolvingElem { get; }

        /// <summary>
        /// Element with state position.
        /// </summary>
        Element StateElem { get; }

        /// <summary>
        /// Show collision in document.
        /// </summary>
        void Show();
    }
}
