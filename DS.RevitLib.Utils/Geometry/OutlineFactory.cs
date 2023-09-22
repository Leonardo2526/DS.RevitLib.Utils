using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Geometry
{
    /// <summary>
    ///  An object is used to create <see cref="Autodesk.Revit.DB.Outline"/>. 
    /// </summary>
    public class OutlineFactory : IOutlineFactory
    {
        private readonly Document _doc;
        private readonly double _defaultOffset = 5000.MMToFeet();
        private readonly double _distnaceToFindFloor = 30;

        private double _xOffset;
        private double _yOffset;
        private double _zOffset;

        /// <summary>
        /// Instansiate an object is used to create <see cref="Autodesk.Revit.DB.Outline"/>. 
        /// </summary>
        /// <param name="doc"></param>
        public OutlineFactory(Document doc)
        {
            _doc = doc;
            _xOffset = _defaultOffset;
            _yOffset = _defaultOffset;
            _zOffset = _defaultOffset;
        }

        /// <summary>
        /// Offset from initial points by X coordinate.
        /// <para>
        /// Default value is 5000mm.
        /// </para>
        /// </summary>
        public double XOffset { get => _xOffset; set => _xOffset = value; }

        /// <summary>
        /// Offset from initial points by Y coordinate.
        /// <para>
        /// Default value is 5000mm.
        /// </para>
        /// </summary>
        public double YOffset { get => _yOffset; set => _yOffset = value; }

        /// <summary>
        /// Offset from initial points by Z coordinate.
        /// <para>
        /// Used when no floor or ceiling was found.
        /// </para>
        /// <para>
        /// Default value is 5000mm.
        /// </para>
        /// </summary>
        public double ZOffset { get => _zOffset; set => _zOffset = value; }

        /// <summary>
        /// Error messages due <see cref="Autodesk.Revit.DB.Outline"/> to creation.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Minimum heigth to floor from initial point.
        /// </summary>
        public double MinHFloor { get; set; }

        /// <summary>
        /// Minimum heigth to ceiling from initial point.
        /// </summary>
        public double MinHCeiling { get; set; }

        /// <summary>
        /// Specifies if outline bound point can be outside floor/ceiling limits.
        /// </summary>
        public bool IsPointEnableOutside { get; set; }

        /// <inheritdoc/>
        public Outline Create(XYZ startPoint, XYZ endPoint)
        {
            ErrorMessage = null;

            XYZ startFloorBound = startPoint.GetXYZBound(_doc, MinHFloor, -_distnaceToFindFloor);
            startFloorBound = RebuildOutsideBound(startFloorBound, startPoint, true);
            if (startFloorBound is null) { return null; }

            XYZ startCeilingBound = startPoint.GetXYZBound(_doc, MinHCeiling, _distnaceToFindFloor);
            startCeilingBound = RebuildOutsideBound(startCeilingBound, startPoint, false);
            if (startCeilingBound is null) { return null; }

            XYZ endFloorBound = endPoint.GetXYZBound(_doc, MinHFloor, -_distnaceToFindFloor);
            endFloorBound = RebuildOutsideBound(endFloorBound, endPoint, true);
            if (endFloorBound is null) { return null; }

            XYZ endCeilingBound = endPoint.GetXYZBound(_doc, MinHCeiling, _distnaceToFindFloor);
            endCeilingBound = RebuildOutsideBound(endCeilingBound, endPoint, false);
            if (endCeilingBound is null) { return null; }

            var (minPoint, maxPoint) = XYZUtils.CreateMinMaxPoints(new List<XYZ>() { startPoint, endPoint });
            var moveVector = new XYZ(XYZ.BasisX.X * _xOffset, XYZ.BasisY.Y * _yOffset, XYZ.BasisZ.Z * _zOffset);

            var p11 = minPoint + moveVector;
            var p12 = minPoint - moveVector;
            (XYZ minP1, XYZ maxP1) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p11, p12 });

            var p21 = maxPoint + moveVector;
            var p22 = maxPoint - moveVector;
            (XYZ minP2, XYZ maxP2) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p21, p22 });

            var minFloorZ = Math.Min(startFloorBound.Z, endFloorBound.Z);
            var maxCeilingZ = Math.Max(startCeilingBound.Z, endCeilingBound.Z);

            minP1 = new XYZ(minP1.X, minP1.Y, minFloorZ);
            maxP2 = new XYZ(maxP2.X, maxP2.Y, maxCeilingZ);

            return new Outline(minP1, maxP2);
        }

        /// <inheritdoc/>
        public Outline Create(XYZ point) => Create(point, point);

        /// <inheritdoc/>
        public Outline CreateManual(UIDocument uiDoc)
        {
            ErrorMessage = null;

            XYZ p1 = uiDoc.Selection.PickPoint("Укажите первую точку зоны.");
            p1.Show(_doc, 200.MMToFeet());
            uiDoc.RefreshActiveView();
            if (p1 == null) { return null; }

            XYZ p2 = uiDoc.Selection.PickPoint("Укажите первую точку зоны.");
            p2.Show(_doc, 200.MMToFeet());
            uiDoc.RefreshActiveView();
            if (p2 == null) { return null; }

            return Create(p1, p2);
        }

        private bool IsValid(XYZ point)
        {
            if (point is null)
            {
                ErrorMessage += "Точка находится вне допустимой зоны перекрытия";
                return false;
            }
            return true;
        }

        private XYZ RebuildFloorZ(XYZ point, double defaultZ) => new(point.X, point.Y, point.Z - defaultZ);
        private XYZ RebuildCeilingZ(XYZ point, double defaultZ) => new(point.X, point.Y, point.Z + defaultZ);

        /// <summary>
        /// Rebuild bound if it's outside floor/ceiling zone or no floor/ceiling was found
        /// </summary>
        /// <param name="bound"></param>
        /// <param name="point"></param>
        /// <param name="floor"></param>
        /// <returns></returns>
        XYZ RebuildOutsideBound(XYZ bound, XYZ point, bool floor)
        {
            Func<XYZ, double, XYZ> rebuild = (p, offset) => floor ?
            RebuildFloorZ(p, offset) :
            RebuildCeilingZ(p, offset);

            if (IsPointEnableOutside && bound == null)
            { return rebuild(point, ZOffset); }
            else if (!IsValid(bound))
            { return null; }

            return bound.Z == double.MaxValue ? rebuild(point, ZOffset) : bound;
        }

    }

}
