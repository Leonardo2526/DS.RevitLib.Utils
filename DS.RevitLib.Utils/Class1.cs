using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils
{
    public static class Class1
    {
        public static bool EqualsV()
        {
            XYZ vector1 = new XYZ(1, 0, 0);
            XYZ vector2 = new XYZ(1, 0, 0);
            return vector1.IsAlmostEqualTo(vector2);
        }
    }
}
