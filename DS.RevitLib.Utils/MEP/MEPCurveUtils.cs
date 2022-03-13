using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP
{
    public static class MEPCurveUtils
    {
        public static double GetLength(MEPCurve mEPCurve)
        {
            LocationCurve lc = mEPCurve.Location as LocationCurve;
            return lc.Curve.ApproximateLength;
        }
    }
}
