using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Net;

namespace DS.RevitLib.Utils.Geometry
{
    /// <summary>
    ///  An object is used to create <see cref="Autodesk.Revit.DB.Outline"/>. 
    /// </summary>
    public class OutlineFactory : IOutlineFactory
    {
        private readonly Document _doc;
        private readonly double _defaultOffset = 5000.MMToFeet();

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


        #region Properties

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
        /// Specifies if outline bound point can be outside floor/ceiling limits.
        /// </summary>
        public bool IsPointEnableOutside { get; set; }

        #endregion

       
        /// <summary>
        /// Create factory.
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <param name="minDistToFloor"></param>
        /// <param name="minDistToCeiling"></param>
        /// <param name="isInsulationAccount"></param>
        /// <returns></returns>
        public Outline CreateManual(UIDocument uiDoc, double minDistToFloor, double minDistToCeiling,
            bool isInsulationAccount = true)
        {
            ErrorMessage = null;

            XYZ p1 = uiDoc.Selection.PickPoint("Укажите первую точку зоны.");
            p1.ShowWithTransaction(_doc, 200.MMToFeet());
            uiDoc.RefreshActiveView();
            if (p1 == null) { return null; }

            XYZ p2 = uiDoc.Selection.PickPoint("Укажите вторую точку зоны.");
            p2.ShowWithTransaction(_doc, 200.MMToFeet());
            uiDoc.RefreshActiveView();
            if (p2 == null) { return null; }

            (Element element, XYZ point) elementPoint1 = (null, p1);
            (Element element, XYZ point) elementPoint2 = (null, p2);

            return Create(elementPoint1, elementPoint2, minDistToFloor, minDistToCeiling, isInsulationAccount);
        }

        /// <inheritdoc/>
        public Outline Create(ConnectionPoint startPoint, ConnectionPoint endPoint)
        {
            (XYZ startFloorBound, XYZ startCeilingBound) = startPoint.FloorBounds;
            startFloorBound = RebuildOutsideBound(startFloorBound, startPoint.Point, true);
            if (startFloorBound is null) { return null; }
            startCeilingBound = RebuildOutsideBound(startCeilingBound, startPoint.Point, false);
            if (startCeilingBound is null) { return null; }

            (XYZ endFloorBound, XYZ endCeilingBound) = startPoint.FloorBounds;
            endFloorBound = RebuildOutsideBound(endFloorBound, endPoint.Point, true);
            if (endFloorBound is null) { return null; }
            endCeilingBound = RebuildOutsideBound(endCeilingBound, endPoint.Point, false);
            if (endCeilingBound is null) { return null; }

            return CreateOutline(startPoint.Point, endPoint.Point, startFloorBound, startCeilingBound, endFloorBound, endCeilingBound);
        }

        /// <summary>
        /// Create factory.
        /// </summary>
        /// <param name="elementPoint"></param>
        /// <param name="minDistToFloor"></param>
        /// <param name="minDistToCeiling"></param>
        /// <param name="isInsulationAccount"></param>
        /// <returns></returns>
        public Outline Create((Element element, XYZ point) elementPoint,
            double minDistToFloor, double minDistToCeiling,
            bool isInsulationAccount = true)
        {
            (XYZ floorBound, XYZ ceilingBound) = elementPoint.GetFloorBounds(_doc, minDistToFloor, minDistToCeiling, isInsulationAccount);
            floorBound = RebuildOutsideBound(floorBound, elementPoint.point, true);
            if (floorBound is null) { return null; }
            ceilingBound = RebuildOutsideBound(ceilingBound, elementPoint.point, false);
            if (ceilingBound is null) { return null; }

            var (minPoint, maxPoint) = XYZUtils.CreateMinMaxPoints(new List<XYZ>() { elementPoint.point, elementPoint.point });
            var moveVector = new XYZ(XYZ.BasisX.X * _xOffset, XYZ.BasisY.Y * _yOffset, XYZ.BasisZ.Z * _zOffset);

            var p11 = minPoint + moveVector;
            var p12 = minPoint - moveVector;
            (XYZ minP1, XYZ maxP1) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p11, p12 });

            var p21 = maxPoint + moveVector;
            var p22 = maxPoint - moveVector;
            (XYZ minP2, XYZ maxP2) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p21, p22 });

            minP1 = new XYZ(minP1.X, minP1.Y, floorBound.Z);
            maxP2 = new XYZ(maxP2.X, maxP2.Y, ceilingBound.Z);

            return new Outline(minP1, maxP2);
        }

        private bool IsValid(XYZ point)
        {
            if (point is null)
            {
                ErrorMessage += "Невозможно определить границы поиска.";
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
        private XYZ RebuildOutsideBound(XYZ bound, XYZ point, bool floor)
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

        private Outline CreateOutline(XYZ p1, XYZ p2, XYZ p1FloorBound, XYZ p1CeilingBound, XYZ p2FloorBound, XYZ p2CeilingBound)
        {
            var (minPoint, maxPoint) = XYZUtils.CreateMinMaxPoints(new List<XYZ>() { p1, p2 });
            var moveVector = new XYZ(XYZ.BasisX.X * _xOffset, XYZ.BasisY.Y * _yOffset, XYZ.BasisZ.Z * _zOffset);

            var p11 = minPoint + moveVector;
            var p12 = minPoint - moveVector;
            (XYZ minP1, XYZ maxP1) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p11, p12 });

            var p21 = maxPoint + moveVector;
            var p22 = maxPoint - moveVector;
            (XYZ minP2, XYZ maxP2) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p21, p22 });

            var minFloorZ = Math.Min(p1FloorBound.Z, p2FloorBound.Z);
            var maxCeilingZ = Math.Max(p1CeilingBound.Z, p2CeilingBound.Z);

            minP1 = new XYZ(minP1.X, minP1.Y, minFloorZ);
            maxP2 = new XYZ(maxP2.X, maxP2.Y, maxCeilingZ);

            return new Outline(minP1, maxP2);
        }

        private Outline Create((Element element, XYZ point) elementPoint1, (Element element, XYZ point) elementPoint2, 
            double minDistToFloor, double minDistToCeiling,
            bool isInsulationAccount = true)
        {
            (XYZ p1FloorBound, XYZ p1CeilingBound) = elementPoint1.GetFloorBounds(_doc, minDistToFloor, minDistToCeiling, isInsulationAccount);
            p1FloorBound = RebuildOutsideBound(p1FloorBound, elementPoint1.point, true);
            if (p1FloorBound is null) { return null; }
            p1CeilingBound = RebuildOutsideBound(p1CeilingBound, elementPoint1.point, false);
            if (p1CeilingBound is null) { return null; }

            (XYZ p2FloorBound, XYZ p2CeilingBound) = elementPoint2.GetFloorBounds(_doc, minDistToFloor, minDistToCeiling, isInsulationAccount);
            p2FloorBound = RebuildOutsideBound(p2FloorBound, elementPoint2.point, true);
            if (p2FloorBound is null) { return null; }
            p2CeilingBound = RebuildOutsideBound(p2CeilingBound, elementPoint2.point, false);
            if (p2CeilingBound is null) { return null; }

            return CreateOutline(elementPoint1.point, elementPoint2.point, p1FloorBound, p1CeilingBound, p2FloorBound, p2CeilingBound);
        }
    }

}
