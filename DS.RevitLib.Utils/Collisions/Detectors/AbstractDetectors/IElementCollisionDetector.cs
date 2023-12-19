using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors
{
    /// <inheritdoc/>
    public interface IElementCollisionDetector : ICollisionDetector<object, Element>, IExclusion
    {      

        /// <summary>
        /// Minimum intersection volume in <see cref="Autodesk.Revit.DB.DisplayUnitType.DUT_CUBIC_CENTIMETERS"/>.
        /// </summary>
        double MinVolume { get; }

        /// <summary>
        /// Specifies whether allow insulation collisions.
        /// </summary>
        bool IsInsulationAccount { get; set; }

        /// <summary>
        /// Current active <see cref="Document"/> <see cref="Autodesk.Revit.DB.Element"/>'s to check collisions.
        /// </summary>
        List<Element> ActiveDocElements { get; set; }

        /// <summary>
        /// <see cref="RevitLinkInstance"/>'s <see cref="Autodesk.Revit.DB.Element"/>'s to check collisions.
        /// </summary>
        List<(RevitLinkInstance, Transform, List<Element>)> LinkElements { get; set; }

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
