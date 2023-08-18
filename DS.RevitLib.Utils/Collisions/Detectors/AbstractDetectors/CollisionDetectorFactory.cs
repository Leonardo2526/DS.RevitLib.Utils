using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Collisions.Detectors
{
    /// <summary>
    /// The interface used to create factories to detect collisions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="P"></typeparam>
    public abstract class CollisionDetectorFactory<T, P> : ICollisionDetector<T,P>
    {

        /// <inheritdoc/>
        public List<(T,P)> Collisions { get; protected set; }


        /// <summary>
        /// Minimum intersection volume in <see cref="Autodesk.Revit.DB.DisplayUnitType.DUT_CUBIC_CENTIMETERS"/>.
        /// Default value is 0.001.
        /// </summary>
        public double MinVolume { get; set; } = 0.001;

        /// <summary>
        /// Get collisions with <paramref name="checkObject1"/>.
        /// </summary>
        /// <param name="checkObject1"></param>
        /// <param name="checkObjects2ToExclude"></param>
        /// <returns></returns>
        public abstract List<(T, P)> GetCollisions(T checkObject1, List<P> checkObjects2ToExclude = null);
    }
}