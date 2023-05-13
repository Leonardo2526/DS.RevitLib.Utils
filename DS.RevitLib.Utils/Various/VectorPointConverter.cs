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
    public class VectorPointConverter : IPointConverter
    {
        private readonly Point3D _uCS1BasePoint;
        private readonly Point3D _uCS2BasePoint;
        private readonly Vector3D _stepVector;

        public VectorPointConverter(Point3D uCS1basePoint, Point3D uCS2basePoint,
            Vector3D stepVector)
        {
            _uCS1BasePoint = uCS1basePoint;
            _uCS2BasePoint = uCS2basePoint;
            _stepVector = stepVector;
        }

        public Point3D ConvertToUSC1(Point3D uCS2Point)
        {
            Vector3D vector =  uCS2Point - _uCS2BasePoint;
            var multiVector = new Vector3D(vector.X * _stepVector.X , vector.Y * _stepVector.Y, vector.Z * _stepVector.Z);
            return _uCS1BasePoint + multiVector;
        }


        public Point3D ConvertToUSC2(Point3D uCS1Point)
        {
            Vector3D vector = uCS1Point - _uCS1BasePoint;
            var multiVector = new Vector3D(vector.X / _stepVector.X, vector.Y / _stepVector.Y,vector.Z / _stepVector.Z);
            return _uCS2BasePoint + multiVector;
        }

    }
}
