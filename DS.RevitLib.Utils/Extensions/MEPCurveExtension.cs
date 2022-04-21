using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Extensions
{
    public static class MEPCurveExtension
    {
        /// <summary>
        /// Check if baseMEPCurve direction is equal to another mEPCurve direction.
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <param name="mEPCurve"></param>
        /// <returns>Return true if directions are equal. Return false if directions aren't equal.</returns>
        public static bool IsEqualDirection(this MEPCurve baseMEPCurve, MEPCurve mEPCurve)
        {
            XYZ mEPCurve1Dir = MEPCurveUtils.GetDirection(baseMEPCurve);
            XYZ mEPCurve2Dir = MEPCurveUtils.GetDirection(mEPCurve);

            double angleRad = mEPCurve1Dir.AngleTo(mEPCurve2Dir);
            double angleDeg = Math.Round(angleRad * 180 / Math.PI, 3);

            if (angleDeg == 0 || angleDeg == 180)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if MEPCurve is rectangular type.
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <param name="mEPCurve"></param>
        /// <returns>Return true if MEPCurve is rectangular type. Return false if it isn't.</returns>
        public static bool IsRecangular(this MEPCurve mEPCurve)
        {
            Document doc = mEPCurve.Document;
            var type = doc.GetElement(mEPCurve.GetTypeId()) as MEPCurveType;
            var shape = type.Shape;
            if (shape is ConnectorProfileType.Rectangular)
            {
                return true;
            }

            return false;
        }
    }
}
