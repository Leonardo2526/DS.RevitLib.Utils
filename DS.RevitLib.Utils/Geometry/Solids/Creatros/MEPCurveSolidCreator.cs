using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using System;

namespace DS.RevitLib.Utils.Solids
{
    /// <summary>
    /// An object to create <see cref="Solid"/> to check collisions at point.
    /// </summary>
    public class MEPCurveSolidCreator : SolidCreatorBase
    {
        private readonly MEPCurve _baseMEPCurve;
        private readonly double _insulationThickness;
        private readonly double _connectedInsulationThickness;
        private readonly double _minConnectedMEPCurveSize;
        private readonly double _minMEPCurveSize;
        private readonly double _elementClearance;

        /// <summary>
        /// Instantiate an object to create <see cref="Solid"/> to check collisions at point.
        /// </summary>
        public MEPCurveSolidCreator(MEPCurve baseMEPCurve, MEPCurve connectedMEPCurve = null, double elementClearance = 0)
        {
            _baseMEPCurve = baseMEPCurve;
            _insulationThickness = baseMEPCurve.GetInsulationThickness();
            (double width, double heigth) = baseMEPCurve.GetOuterWidthHeight();
            _minMEPCurveSize = Math.Min(width, heigth);

            _elementClearance = elementClearance;
            connectedMEPCurve ??= baseMEPCurve;
            _connectedInsulationThickness = connectedMEPCurve.GetInsulationThickness();
            (double cwidth, double cheigth) = connectedMEPCurve.GetOuterWidthHeight();
            _minConnectedMEPCurveSize = Math.Min(cwidth, cheigth);
        }

        /// <summary>
        /// Create <see cref="Solid"/> at given <paramref name="point"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override Solid CreateSolid(XYZ point)
        {
            double distFromPoint= _minMEPCurveSize / 2 + _insulationThickness + _elementClearance;
            double connectedDistFromPoint = _minConnectedMEPCurveSize / 2 + _connectedInsulationThickness + _elementClearance;
            double max = Math.Max(distFromPoint, connectedDistFromPoint);

            var dir = MEPCurveUtils.GetDirection(_baseMEPCurve);
            return Solid = _baseMEPCurve.GetOffsetSolid(max  - _minMEPCurveSize /2,
                point - dir.Multiply(connectedDistFromPoint),
                point + dir.Multiply(connectedDistFromPoint));
        }
    }
}

