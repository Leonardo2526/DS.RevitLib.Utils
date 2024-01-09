using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Various extecnsion methods
    /// </summary>
    public static class ConvertationExtensions
    {
        /// <summary>
        /// Convert feets to millimeters
        /// </summary>
        /// <param name="value">Value in millimeters</param>
        /// <returns>Returns value in millimeters.</returns>
        public static double FeetToMM(this double value)
        {
            return UnitUtils.Convert(value, DisplayUnitType.DUT_DECIMAL_FEET, DisplayUnitType.DUT_MILLIMETERS);
        }

        /// <summary>
        /// Convert feets to millimeters
        /// </summary>
        /// <param name="value">Value in millimeters</param>
        /// <returns>Returns value in millimeters.</returns>
        public static double FeetToMM(this int value)
        {
            double doubleValue = (double)value;
            return doubleValue.FeetToMM();
        }

        /// <summary>
        /// Convert millimeters to feets.
        /// </summary>
        /// <param name="value">Value in feets</param>
        /// <returns>Returns value in feets.</returns>
        public static double MMToFeet(this double value)
        {
            return UnitUtils.Convert(value, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_DECIMAL_FEET);
        }

        /// <summary>
        /// Convert millimeters to feets.
        /// </summary>
        /// <param name="value">Value in feets</param>
        /// <returns>Returns value in feets.</returns>
        public static double MMToFeet(this int value)
        {
            double doubleValue = (double)value;
            return doubleValue.MMToFeet();
        }
    }
}
