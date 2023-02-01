using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

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
        /// Swap MEPCurve's width and height.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="trb"></param>
        /// <returns>Returns MEPCurve with swaped parameters.</returns>
        public static MEPCurve SwapSize(this MEPCurve mEPCurve, AbstractTransactionBuilder trb = null)
        {
            Document doc = mEPCurve.Document;
            void action()
            {
                double width = mEPCurve.Width;
                double height = mEPCurve.Height;

                Parameter widthParam = mEPCurve.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
                Parameter heightParam = mEPCurve.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM);

                widthParam.Set(height);
                heightParam.Set(width);
            }

            if (!doc.IsModifiable)
            {
                trb ??= new TransactionBuilder(doc);
                trb.Build(action, "FixOrientation");
            }
            else
            { action(); }

            return mEPCurve;
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
        public static double GetMaxSize(this MEPCurve mEPCurve)
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
        public static bool IsRectangular(this MEPCurve mEPCurve)
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

        /// <summary>
        /// Get MEPCurveType object.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Return MEPCurveType object if MEPCurveType is Pipe or Duct.</returns>
        public static MEPCurveType GetMEPCurveType(this MEPCurve mEPCurve)
        {
            ElementType elementType = mEPCurve.GetElementType2();

            if (mEPCurve.GetType().Name == "Pipe")
            {
                return elementType as PipeType;
            }
            else if (mEPCurve.GetType().Name == "Duct")
            {
                return elementType as DuctType;
            }

            return null;
        }

        /// <summary>
        /// Get offseted solid from <paramref name="mEPCurve"/>.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="offset">Offset distance in feets. If value is positive offset will be outside of <paramref name="mEPCurve"/>, 
        /// and inside if negative. </param>
        /// <returns>Returns offseted solid from <paramref name="mEPCurve"/> with specified offset distance.</returns>
        public static Solid GetOffsetSolid(this MEPCurve mEPCurve, double offset)
        {
            return new SolidOffsetExtractor(mEPCurve, offset).Extract();
        }

        /// <summary>
        /// Get offseted solid from <paramref name="mEPCurve"/>.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="offset">Offset distance in feets. If value is positive offset will be outside of <paramref name="mEPCurve"/>, 
        /// and inside if negative. </param>
        /// <param name="startPoint">Start point of solid extrusion.</param>
        /// <param name="endPoint">End point of solid extrusion.</param>
        /// <returns>Returns offseted solid from <paramref name="mEPCurve"/> 
        /// between <paramref name="startPoint"/> and <paramref name="endPoint"/> with specified offset distance.</returns>
        public static Solid GetOffsetSolid(this MEPCurve mEPCurve, double offset, XYZ startPoint, XYZ endPoint)
        {
            return new SolidOffsetExtractor(mEPCurve, offset, startPoint, endPoint).Extract();
        }

        /// <summary>
        /// Specify if <paramref name="mEPCurve"/> has valid sizes.
        /// </summary>
        /// <remarks>If <paramref name="mEPCurve"/> is not rectangular or has equal width and height </remarks>
        /// <param name="mEPCurve"></param>
        /// <returns>Returns <see langword="true"></see> if <paramref name="mEPCurve"/>'s size by <see cref="Autodesk.Revit.DB.XYZ.Z"/>
        /// is equal to <paramref name="mEPCurve"/>'s height property. Otherwize returns <see langword="false"></see>.
        /// <para>Returns <see langword="true"></see> if <paramref name="mEPCurve"/> is not rectangular or has equal width and height.</para>
        /// <para>Returns <see langword="true"></see> if zOrth of <paramref name="mEPCurve"/> is null.</para>
        /// </returns>
        public static bool HasValidOrientation(this MEPCurve mEPCurve)
        {
            if (mEPCurve.GetProfileType() != ConnectorProfileType.Rectangular || mEPCurve.Height == mEPCurve.Width)
            { return true; }

            var orths = ElementUtils.GetOrthoNormVectors(mEPCurve);
            var zOrth = orths.FirstOrDefault(obj => XYZUtils.Collinearity(XYZ.BasisZ, obj));
            if(zOrth == null) 
            { Debug.WriteLine($"Warning: failed to check MEPCurve {mEPCurve.Id} orientation."); return true; }

            var height = mEPCurve.GetSizeByVector(zOrth) * 2;

            return Math.Round(height, 3) == Math.Round(mEPCurve.Height, 3);
        }

        /// <summary>
        /// Fix <paramref name="mEPCurve"/> if it has not valid orientation.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="trb"></param>
        public static void FixNotValidOrientation(this MEPCurve mEPCurve, AbstractTransactionBuilder trb = null)
        {
            if(mEPCurve.HasValidOrientation()) { return; }

            Document doc = mEPCurve.Document;
            void action()
            {
                ElementTransformUtils.RotateElement(doc, mEPCurve.Id, mEPCurve.GetCenterLine(), Math.PI / 2);
                mEPCurve.SwapSize(trb);
            }

            if (!doc.IsModifiable)
            {
                trb ??= new TransactionBuilder(doc);
                trb.Build(action, "FixOrientation");
            }
            else
            { action(); }
        }
    }
}
