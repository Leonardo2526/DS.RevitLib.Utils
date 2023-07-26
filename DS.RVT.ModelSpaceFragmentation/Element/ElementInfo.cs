using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Points;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace DS.RVT.ModelSpaceFragmentation
{
    class ElementInfo
    {
        /// <summary>
        /// Minimum point of zone for fragmentation
        /// </summary>
        public static XYZ MinBoundPoint { get; set; }
        /// <summary>
        /// Maximium point of zone for fragmentation
        /// </summary>
        public static XYZ MaxBoundPoint { get; set; }
        public static double OffsetFromOriginByX { get; } = 2000;
        public static double OffsetFromOriginByY { get; } = 2000;
        public static double OffsetFromOriginByZ { get; } = 2000;

        public static XYZ StartElemPoint { get; set; }
        public static XYZ EndElemPoint { get; set; }

        private readonly OrthoNormBasis _basis;
        private readonly XYZ _startPoint;
        private readonly XYZ _endPoint;

        public ElementInfo(OrthoNormBasis basis, XYZ startPoint, XYZ endPoint)
        {
            _basis = basis;
            _startPoint = startPoint;
            _endPoint = endPoint;
        }

        public List<XYZ> GetPoints()
        {
            List<XYZ> elementPoints = new List<XYZ>();
            elementPoints.Add(_startPoint);
            elementPoints.Add(_endPoint);
            StartElemPoint = _startPoint;
            EndElemPoint = _endPoint;

            //GetOffset();

            PointUtils pointUtils = new PointUtils();
            pointUtils.FindMinMaxPointByPoints(elementPoints, out XYZ minPoint, out XYZ maxPoint);

            List<XYZ> boundPoints = new List<XYZ>();

            Vector3D moveVector = _basis.X + _basis.Y + _basis.Z;
            var xYZMoveVector = new XYZ(
                moveVector.X * OffsetFromOriginByX.MMToFeet(), 
                moveVector.Y * OffsetFromOriginByY.MMToFeet(), 
                moveVector.Z * OffsetFromOriginByZ.MMToFeet());

            var p1 = minPoint + xYZMoveVector;
            var p2 = minPoint - xYZMoveVector;

            pointUtils.FindMinMaxPointByPoints(new List<XYZ>() { p1, p2}, out XYZ minOffsetPoint, out XYZ maxOffsetPoint);
            MinBoundPoint = minOffsetPoint;

            p1 = maxPoint + xYZMoveVector;
            p2 = maxPoint - xYZMoveVector;

            pointUtils.FindMinMaxPointByPoints(new List<XYZ>() { p1, p2 }, out minOffsetPoint, out maxOffsetPoint);
            MaxBoundPoint = maxOffsetPoint;

            boundPoints.Add(MinBoundPoint);
            boundPoints.Add(MaxBoundPoint);

            return boundPoints;
        }
    }
}
