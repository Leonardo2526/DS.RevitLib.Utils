using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class MEPCurveCreator
    {
        private readonly Document Doc;

        public MEPCurveCreator(Document doc)
        {
            Doc = doc;
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
                return new FilteredElementCollector(Doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault().Id;
            }
        }

        #endregion


        /// <summary>
        /// Create pipe between 2 points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public MEPCurve CreateMEPCurveByPoints(XYZ p1, XYZ p2, MEPCurve baseMEPCurve)
        {
            MEPCurve mEPCurve = null;
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
                    }

                    Insulation.Create(baseMEPCurve, mEPCurve);
                    MEPCurveParameter.Copy(baseMEPCurve, mEPCurve);
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

       
    }
}
