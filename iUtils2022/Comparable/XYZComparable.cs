using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iUtils.Comparable
{
    public class XYZComparable : IComparable
    {
        public XYZ Point{ get; set; }

        public XYZComparable(XYZ point)
        {
            Point = point ?? throw new ArgumentNullException(nameof(point));
        }

        public int CompareTo(object obj)
        {
            var len = Point.GetLength();
            var objLen =((XYZComparable) obj).Point.GetLength();
            var difference = Math.Abs(len - objLen);
            if (difference < 1e-8)
                return 0;
            else if (len > objLen)
                return 1;
            else
                return -1;
        } 
    }
}
