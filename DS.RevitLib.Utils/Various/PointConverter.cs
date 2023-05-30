using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Points;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace DS.RevitLib.Utils.Various
{
    public class PointConverter : IPointConverter
    {
        private readonly Point3D _uCS1BasePoint;
        private readonly Point3D _uCS2BasePoint;
        private readonly double _convertCoefficient;

        public PointConverter(Point3D uCS1basePoint, Point3D uCS2basePoint,
            double convertCoefficient)
        {
            _uCS1BasePoint = uCS1basePoint;
            _uCS2BasePoint = uCS2basePoint;
            _convertCoefficient = convertCoefficient;
        }

        public Point3D ConvertToUCS1(Point3D uCS2Point)
        {
            Vector3D vector =  uCS2Point - _uCS2BasePoint;
            return _uCS1BasePoint + vector.Multiply(_convertCoefficient);
        }


        public Point3D ConvertToUCS2(Point3D uCS1Point)
        {
            Vector3D vector = uCS1Point - _uCS1BasePoint;
            return _uCS2BasePoint + vector.Multiply(1/ _convertCoefficient);
        }

        public Point3D Round(Point3D point)
        {
            return new Point3D((int)Math.Round(point.X), (int)Math.Round(point.Y), (int)Math.Round(point.Z));
        }

    }
}
