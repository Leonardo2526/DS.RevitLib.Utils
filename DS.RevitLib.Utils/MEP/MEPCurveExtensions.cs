using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.MEP
{
    /// <summary>
    /// MEPCurve's extensions methods
    /// </summary>
    public static class MEPCurveExtensions
    {
        /// <summary>
        /// Cut MEPCurve between points.
        /// </summary>
        /// <returns>Returns splitted MEPCurves</returns>
        public static List<MEPCurve> Cut(this MEPCurve mEPCurve, XYZ point1, XYZ point2)
        {
            var cutter = new MEPCurveCutter(mEPCurve, point1, point2);
            return cutter.Cut();
        }

        /// <summary>
        /// Get Basis from MEPCurve.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Returns basis in centerPoint of MEPCurve.</returns>
        public static Basis GetBasis(this MEPCurve mEPCurve)
        {
            Line line = MEPCurveUtils.GetLine(mEPCurve);

            var basisX = line.Direction;
            var orths = ElementUtils.GetOrthoNormVectors(mEPCurve);
            var basisY = ElementUtils.GetMaxSizeOrth(mEPCurve, orths);
            var basisZ = basisX.CrossProduct(basisY);
            Basis basis = new Basis(basisX, basisY, basisZ, line.GetCenter());
            basis.Round();

            return basis;
        }
    }
}
