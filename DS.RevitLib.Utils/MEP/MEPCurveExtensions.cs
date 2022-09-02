using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Creator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP
{
    public static class MEPCurveExtensions
    {
        /// <summary>
        /// Cut MEPCurve between points.
        /// </summary>
        /// <returns>Returns splitted MEPCurves</returns>
        public static List<MEPCurve> Cut(this MEPCurve mEPCurve, XYZ point1, XYZ point2)
        {
            var (mEPCurveCon1, mEPCurveCon2) = ConnectorUtils.GetMainConnectors(mEPCurve);
            if (!point1.IsBetweenPoints(mEPCurveCon1.Origin, mEPCurveCon2.Origin) |
               !point2.IsBetweenPoints(mEPCurveCon1.Origin, mEPCurveCon2.Origin))
            {
                return null;
            }

            var cutter = new MEPCurveCutter(mEPCurve, point1, point2);
            return cutter.Cut();
        }
    }
}
