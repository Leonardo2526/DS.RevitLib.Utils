using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Connections.PointModels
{
    /// <summary>
    /// Class for object to describe a point for <see cref="Autodesk.Revit.DB.MEPCurve"/>'s connection 
    /// </summary>
    public class BestMEPCurveConnectionPoint : ConnectionPoint
    {
        /// <summary>
        /// Initiate a new object to connect <paramref name="mEPCurves"/> at <paramref name="point"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="element"/> specifies <see cref="Autodesk.Revit.DB.Element"/> at <paramref name="point"/>.
        /// <para>
        /// <paramref name="mEPCurves"/> specifies <see cref="MEPCurve"/>'s to connect.        
        /// </para>
        /// </remarks>
        /// <param name="element"><see cref="Autodesk.Revit.DB.Element"/> at <paramref name="point"/>.</param>
        /// <param name="point">Connection point</param>
        /// <param name="mEPCurves">MEPCurves to connect</param>
        public BestMEPCurveConnectionPoint(Element element, XYZ point, List<MEPCurve> mEPCurves) : base(element, point)
        {
            MEPCurves = mEPCurves;
        }

        /// <summary>
        /// MEPCurves to connect
        /// </summary>
        public List<MEPCurve> MEPCurves { get; }
    }
}
