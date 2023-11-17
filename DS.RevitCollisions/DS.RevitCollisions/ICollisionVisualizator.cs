using DS.RevitCollisions.Models;

namespace DS.RevitCollisions
{
    /// <summary>
    /// The interface used to show collisions.
    /// </summary>
    public interface ICollisionVisualizator
    {
        /// <summary>
        /// Show <paramref name="collision"/> in document.
        /// </summary>
        /// <param name="collision">Collision to show in document.</param>
        void Show(IElementCollision collision);
    }
}
