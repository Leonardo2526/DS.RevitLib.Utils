using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using DS.RevitLib.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Get max size between width and height of <paramref name="mEPCurve"/>.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns></returns>
        public static double  GetMaxSize(this MEPCurve mEPCurve)
        {
            (double width, double heigth) = MEPCurveUtils.GetWidthHeight(mEPCurve);
            return Math.Max(width, heigth);
        }

        /// <summary>
        /// Get mEPCurve's insulation thickness.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Return thickness.</returns>
        public static double GetInsulationThickness(this MEPCurve mEPCurve)
        {
            var insulations = InsulationLiningBase.GetInsulationIds(mEPCurve.Document, mEPCurve.Id)
                .Select(x => mEPCurve.Document.GetElement(x) as InsulationLiningBase).ToList();

            if (insulations != null && insulations.Any())
            {
                return insulations.First().Thickness;
            }

            return 0;
        }

        /// <summary>
        /// Get outer sizes of MEPCurve.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Returns actual width and height of recrangle and diameter of round profile.</returns>
        public static (double width, double heigth) GetOuterWidthHeight(this MEPCurve mEPCurve)
        {
            double width = 0;
            double heigth = 0;

            ConnectorProfileType connectorProfileType = mEPCurve.GetProfileType();
            switch (connectorProfileType)
            {
                case ConnectorProfileType.Invalid:
                    break;
                case ConnectorProfileType.Round:
                    {
                        Parameter diameter = mEPCurve.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
                        width = diameter.AsDouble();
                        heigth = width;
                    }
                    break;
                case ConnectorProfileType.Rectangular:
                    {
                        width = mEPCurve.Width;
                        heigth = mEPCurve.Height;
                    }
                    break;
                case ConnectorProfileType.Oval:
                    break;
                default:
                    break;
            }

            return (width, heigth);
        }

        /// <summary>
        /// Get <see cref="MEPCurve"/> profile type.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns></returns>
        public static ConnectorProfileType GetProfileType(this MEPCurve mEPCurve)
        {
            var doc = mEPCurve.Document;
            var type = doc.GetElement(mEPCurve.GetTypeId()) as MEPCurveType;
            return type.Shape;
        }
    }
}
