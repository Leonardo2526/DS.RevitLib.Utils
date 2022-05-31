using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using DS.MainUtils;
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

        public MEPCurveCreator(Document doc, MEPCurve baseMEPCurve)
        {
            Doc = doc;
            BaseMEPCurve = baseMEPCurve;
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
            //var bcons = ConnectorUtils.GetConnectors(baseMEPCurve);
            //Connector con = ConnectorUtils.GetClosest(p1, bcons);


            using (Transaction transNew = new Transaction(Doc, "CreateMEPCurveByPoints"))
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
                        //mEPCurve = Duct.Create(Doc, ElementTypeId, MEPLevelId, con, p2);
                    }
               
                    Insulation.Create(baseMEPCurve, mEPCurve);
                    MEPCurveParameter.Copy(baseMEPCurve, mEPCurve);
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
        /// Create pipe between 2 connectors
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public MEPCurve CreateMEPCurveByConnectors(Connector c1, Connector c2)
        {
            MEPCurve mEPCurve = null;
            using (Transaction transNew = new Transaction(Doc, "CreateMEPCurveByConnectors"))
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
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }

            return mEPCurve;
        }

        public static Element SplitElement(Document Doc, Element element, XYZ splitPoint)
        {
            Element newElement = null;
            var elementTypeName = element.GetType().Name;

            using (Transaction transNew = new Transaction(Doc, "SplitElement"))
            {
                try
                {
                    transNew.Start();

                    ElementId newCurveId;
                    if (elementTypeName == "Pipe")
                    {
                        newCurveId = PlumbingUtils.BreakCurve(Doc, element.Id, splitPoint);
                    }
                    else
                    {
                        newCurveId = MechanicalUtils.BreakCurve(Doc, element.Id, splitPoint);
                    }
                    newElement = Doc.GetElement(newCurveId);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
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
        public static MEPCurve Rotate(MEPCurve mEPCurve, double angle)
        {
            using (Transaction transNew = new Transaction(mEPCurve.Document, "RotateMEPCurve"))
            {
                try
                {
                    transNew.Start();

                    var locCurve = mEPCurve.Location as LocationCurve;
                    var line = locCurve.Curve as Line;

                    mEPCurve.Location.Rotate(line, angle);
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
        /// Swap MEPCurve's width and height.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Return MEPCurve with swaped parameters.</returns>
        public static MEPCurve SwapSize(MEPCurve mEPCurve)
        {
            double width = mEPCurve.Width;
            double height = mEPCurve.Height;

            using (Transaction transNew = new Transaction(mEPCurve.Document, "CreateMEPCurveByPoints"))
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


      

    }
}
