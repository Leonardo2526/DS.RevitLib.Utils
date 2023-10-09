using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors
{
    /// <inheritdoc/>
    public interface IElementCollisionDetector : ICollisionDetector<object, Element>
    {
        /// <summary>
        /// Minimum intersection volume in <see cref="Autodesk.Revit.DB.DisplayUnitType.DUT_CUBIC_CENTIMETERS"/>.
        /// </summary>
        public double MinVolume { get; }

        /// <summary>
        /// Get collisions of <paramref name="checkObject"/>.
        /// </summary>
        List<(Element, Element)> GetCollisions(Element checkObject);

        /// <summary>
        /// Get collisions of <paramref name="checkObject"/>.
        /// </summary>
        List<(Solid, Element)> GetCollisions(Solid checkObject);
    }
}
