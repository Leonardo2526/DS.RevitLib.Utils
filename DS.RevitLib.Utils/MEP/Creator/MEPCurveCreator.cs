﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class MEPCurveCreator
    {
        private readonly Document Doc;
        private readonly MEPCurve BaseMEPCurve;

        public MEPCurveCreator(MEPCurve baseMEPCurve, string transactionPrefix = "")
        {
            Doc = baseMEPCurve.Document;
            BaseMEPCurve = baseMEPCurve;

            if (!String.IsNullOrEmpty(transactionPrefix))
            {
                TransactionPrefix = transactionPrefix + "_";
            }
        }


        #region Properties

        private ElementId MEPSystemTypeId
        {
            get
            {
                MEPCurve mEPCurve = BaseMEPCurve as MEPCurve;
                return mEPCurve.MEPSystem.GetTypeId();
            }
        }
        private ElementId ElementTypeId
        {
            get
            {
                return BaseMEPCurve.GetTypeId();
            }
        }
        private string ElementTypeName
        {
            get { return BaseMEPCurve.GetType().Name; }
        }
        private ElementId MEPLevelId
        {
            get
            {
                return BaseMEPCurve.ReferenceLevel.Id;
            }
        }

        public string ErrorMessages { get; private set; }

        private readonly string TransactionPrefix;

        #endregion


        /// <summary>
        /// Create pipe between 2 points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public MEPCurve CreateMEPCurveByPoints(XYZ p1, XYZ p2, MEPCurve baseMEPCurve = null)
        {
            MEPCurve mEPCurve = null;

            if(baseMEPCurve is null)
            {
                baseMEPCurve = BaseMEPCurve;
            }

            using (Transaction transNew = new Transaction(Doc, TransactionPrefix + "CreateMEPCurveByPoints"))
            {
                try
                {
                    transNew.Start();
                    if (ElementTypeName == "Pipe")
                    {
                        mEPCurve = Pipe.Create(Doc, MEPSystemTypeId, ElementTypeId, MEPLevelId, p1, p2);
                    }
                    else
                    {
                        mEPCurve = Duct.Create(Doc, MEPSystemTypeId, ElementTypeId, MEPLevelId, p1, p2);
                    }
               
                    Insulation.Create(baseMEPCurve, mEPCurve);
                    MEPCurveParameter.Copy(baseMEPCurve, mEPCurve);
                }
                catch (Exception e)

                { ErrorMessages += e + "\n"; }
                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }
            return mEPCurve;
        }

        /// <summary>
        /// Create pipe between 2 connectors
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public MEPCurve CreateMEPCurveByConnectors(Connector c1, Connector c2)
        {
            MEPCurve mEPCurve = null;
            using (Transaction transNew = new Transaction(Doc, TransactionPrefix + "CreateMEPCurveByConnectors"))
            {
                try
                {
                    transNew.Start();
                    if (ElementTypeName == "Pipe")
                    {
                        mEPCurve = Pipe.Create(Doc, MEPSystemTypeId, ElementTypeId, c1, c2);
                    }
                    else
                    {
                        mEPCurve = Duct.Create(Doc, MEPSystemTypeId, ElementTypeId, c1, c2);
                    }

                    Insulation.Create(BaseMEPCurve, mEPCurve);
                    MEPCurveParameter.Copy(BaseMEPCurve, mEPCurve);
                }

                catch (Exception e)
                { ErrorMessages += e + "\n"; }
                transNew.Commit();
            }

            return mEPCurve;
        }

        public Element SplitElement(XYZ splitPoint)
        {
            Element newElement = null;
            var elementTypeName = BaseMEPCurve.GetType().Name;

            using (Transaction transNew = new Transaction(Doc, TransactionPrefix + "SplitElement"))
            {
                try
                {
                    transNew.Start();

                    ElementId newCurveId;
                    if (elementTypeName == "Pipe")
                    {
                        newCurveId = PlumbingUtils.BreakCurve(Doc, BaseMEPCurve.Id, splitPoint);
                    }
                    else
                    {
                        newCurveId = MechanicalUtils.BreakCurve(Doc, BaseMEPCurve.Id, splitPoint);
                    }
                    newElement = Doc.GetElement(newCurveId);
                }

                catch (Exception e)
                { ErrorMessages += e + "\n"; }
                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return newElement;
        }

        /// <summary>
        /// Rotate MEPCurve by angle in rads.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="angle"></param>
        /// <returns>Return rotated MEPCurve.</returns>
        public MEPCurve Rotate(double angle)
        {
            using (Transaction transNew = new Transaction(Doc, TransactionPrefix + "RotateMEPCurve"))
            {
                try
                {
                    transNew.Start();

                    var locCurve = BaseMEPCurve.Location as LocationCurve;
                    var line = locCurve.Curve as Line;

                    BaseMEPCurve.Location.Rotate(line, angle);
                }
                catch (Exception e)
                { ErrorMessages += e + "\n"; }
                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return BaseMEPCurve;
        }

        /// <summary>
        /// Swap MEPCurve's width and height.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Return MEPCurve with swaped parameters.</returns>
        public MEPCurve SwapSize(MEPCurve mEPCurve)
        {
            double width = mEPCurve.Width;
            double height = mEPCurve.Height;

            using (Transaction transNew = new Transaction(Doc, TransactionPrefix + "CreateMEPCurveByPoints"))
            {
                try
                {
                    transNew.Start();

                    Parameter widthParam = mEPCurve.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
                    Parameter heightParam = mEPCurve.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM);

                    widthParam.Set(height);
                    heightParam.Set(width);
                }

                catch (Exception e)
                { }

                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }
            return mEPCurve;
        }


        /// <summary>
        /// Move MEPCurve by specified vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns>Return moved MEPCurve.</returns>
        public MEPCurve Move(XYZ vector)
        {
            using (Transaction transNew = new Transaction(Doc, TransactionPrefix + "MoveMEPCurve"))
            {
                try
                {
                    transNew.Start();
                    ElementTransformUtils.MoveElement(Doc, BaseMEPCurve.Id, vector);
                }
                catch (Exception e)
                { ErrorMessages += e + "\n"; }
                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return BaseMEPCurve;
        }

    }
}
