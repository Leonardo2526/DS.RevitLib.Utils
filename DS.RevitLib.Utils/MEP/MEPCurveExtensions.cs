using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
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
        public static List<MEPCurve> Cut(this MEPCurve mEPCurve, XYZ point1, XYZ point2, bool transactionCommit = false)
        {
            var cutter = new MEPCurveCutter(mEPCurve, point1, point2, transactionCommit);
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

        /// <summary>
        /// Split given <paramref name="mEPCurve"/> in <paramref name="point"/>.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="point"></param>
        /// <returns>Returns a new created MEPCurve.</returns>
        public static MEPCurve Split(this MEPCurve mEPCurve, XYZ point)
        {
            Document doc = mEPCurve.Document;

            var elementTypeName = mEPCurve.GetType().Name;
            ElementId newCurveId = elementTypeName == "Pipe" ?
                PlumbingUtils.BreakCurve(doc, mEPCurve.Id, point) :
                MechanicalUtils.BreakCurve(doc, mEPCurve.Id, point);

            return doc.GetElement(newCurveId) as MEPCurve;
        }
    }
}
