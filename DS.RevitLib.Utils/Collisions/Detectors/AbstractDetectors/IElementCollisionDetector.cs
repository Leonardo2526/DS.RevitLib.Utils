using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors
{
    /// <inheritdoc/>
    public interface IElementCollisionDetector : ICollisionDetector<object, Element>
    {
        /// <summary>
        /// <see cref="Autodesk.Revit.DB.Element"/>'s to exclude from collisions.
        /// </summary>
        List<Element> ExludedElements { get; set; }

        /// <summary>
        /// Minimum intersection volume in <see cref="Autodesk.Revit.DB.DisplayUnitType.DUT_CUBIC_CENTIMETERS"/>.
        /// </summary>
        double MinVolume { get; }

        /// <summary>
        /// Get collisions of <paramref name="checkObject"/>.
        /// </summary>
        List<(Element, Element)> GetCollisions(Element checkObject);

        /// <summary>
        /// Get collisions of <paramref name="checkObject"/>.
        /// </summary>
        List<(Solid, Element)> GetCollisions(Solid checkObject);

        /// <summary>
        /// Update <see cref="Autodesk.Revit.DB.Element"/>'s to check collisions from active <see cref="Document"/>.
        /// </summary>
        /// <param name="activeDocElements"></param>
        void Rebuild(List<Element> activeDocElements);
    }
}
