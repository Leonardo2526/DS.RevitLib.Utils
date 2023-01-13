using DS.RevitLib.Utils.Collisions.Models;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Collisions.Detectors
{
    /// <summary>
    /// The interface used to create objects for collisions (intersections) detection between objects in Revit model.
    /// </summary>
    public interface ICollisionDetector
    {
        /// <summary>
        /// Detected collisions between objects in Revit model.
        /// </summary>
        List<ICollision> Collisions { get; }
    }
}
