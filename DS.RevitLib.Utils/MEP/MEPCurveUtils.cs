using Autodesk.Revit.DB;
using Ivanov.RevitLib.Utils;
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

        /// <summary>
        /// Get intersercion between two MEPCurves. 
        /// </summary>
        /// <param name="mEPCurve1"></param>
        /// <param name="mEPCurve2"></param>
        /// <returns>Return real or virtual (in 10 feets range) intersection point. Return null if no intersection exist.</returns>
        public static XYZ GetIntersection(MEPCurve mEPCurve1, MEPCurve mEPCurve2)
        {
            Line line1 = mEPCurve1.GetCurve() as Line;
            Line line2 = mEPCurve2.GetCurve() as Line;

            line1 = line1.IncreaseLength(10);
            line2 = line2.IncreaseLength(10);

            line1.Intersect(line2, out IntersectionResultArray resultArray);

            if (resultArray is null)
            {
                return null;
            }

            return resultArray.get_Item(0).XYZPoint;
        }


        public static XYZ GetDirection(MEPCurve mEPCurve)
        {
            var locCurve = mEPCurve.Location as LocationCurve;
            var line = locCurve.Curve as Line;

            return line.Direction;
        }

        public static XYZ GetVector(MEPCurve mEPCurve)
        {
            var locCurve = mEPCurve.Location as LocationCurve;
            var line = locCurve.Curve as Line;
            var vector = line.GetEndPoint(1) - line.GetEndPoint(0);

            return vector;
        }


        /// <summary>
        /// Swap MEPCurve's width and height.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Return MEPCurve with swaped parameters.</returns>
        public static MEPCurve SwapSize(MEPCurve mEPCurve)
        {
            double width = mEPCurve.Width;
            double height = mEPCurve.Height;

            Parameter widthParam = mEPCurve.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
            Parameter heightParam = mEPCurve.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM);

            widthParam.Set(height);
            heightParam.Set(width);

            return mEPCurve;
        }
    }
}
