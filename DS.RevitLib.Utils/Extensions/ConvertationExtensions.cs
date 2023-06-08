using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

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
            return UnitUtils.Convert(value, UnitTypeId.Feet, UnitTypeId.Millimeters);
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
            return UnitUtils.Convert(value, UnitTypeId.Millimeters, UnitTypeId.Feet);
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

        /// <summary>
        /// Convert <paramref name="point"/> to <see cref="Autodesk.Revit.DB.XYZ"/>.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Returns a new <see cref="Autodesk.Revit.DB.XYZ"/> built by <paramref name="point"/> coordinates.</returns>
        public static XYZ ToXYZ(this Point3D point)
        {
            return new XYZ(point.X, point.Y, point.Z);
        }
    }
}
