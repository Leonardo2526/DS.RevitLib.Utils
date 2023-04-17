using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace DS.RevitLib.Utils.Various
{
    public interface IPointConverter
    {
        Point3D ConvertToUSC1(Point3D uCS2Point);
        Point3D ConvertToUSC2(Point3D uCS1Point);
    }
}
