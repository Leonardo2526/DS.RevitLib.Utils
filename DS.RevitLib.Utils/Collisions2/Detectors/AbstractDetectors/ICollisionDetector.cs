using System.Collections.Generic;

namespace DS.RevitLib.Collisions2
{
    /// <summary>
    /// The interface used to create objects for collisions (intersections) detection between objects in Revit model.
    /// </summary>
    public interface ICollisionDetector
    {
        /// <summary>
        /// Detected collisions between objects in Revit model.
        /// </summary>
        List<IBestCollision> Collisions { get; }
    }
}
