using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors
{
    /// <inheritdoc/>
    public interface IXYZCollisionDetector : ICollisionDetector<XYZ, Element>
    {
        /// <summary>
        /// Get collisions of <paramref name="point"/>.
        /// </summary>
        List<(XYZ, Element)> GetCollisions(XYZ point);

        /// <summary>
        /// Set <paramref name="mEPCurve"/> to check collisions at point.
        /// </summary>
        /// <param name="mEPCurve"></param>
        IXYZCollisionDetector SetMEPCurve(MEPCurve mEPCurve);
    }
}
