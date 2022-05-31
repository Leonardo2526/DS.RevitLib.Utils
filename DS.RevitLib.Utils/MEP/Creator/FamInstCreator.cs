using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class FamInstCreator
    {
        public FamInstCreator(Document doc)
        {
            Doc = doc;
        }

        #region Fields

        private readonly Document Doc;

        #endregion

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

        private Level MEPLevel
        {
            get
            {
                return new FilteredElementCollector(Doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault();
            }
        }



        /// <summary>
        /// Create fitting between two pipes
        /// </summary>
        /// <param name="mepCurve1"></param>
        /// <param name="mepCurve2"></param>
        public FamilyInstance CreateFittingByMEPCurves(MEPCurve mepCurve1, MEPCurve mepCurve2)
        {
            FamilyInstance familyInstance = null;
            using (Transaction transNew = new Transaction(Doc, "CreateFittingByMEPCurves"))
            {
                try
                {
                    transNew.Start();

                    List<Connector> connectors1 = ConnectorUtils.GetConnectors(mepCurve1);
                    List<Connector> connectors2 = ConnectorUtils.GetConnectors(mepCurve2);

                    ConnectorUtils.GetNeighbourConnectors(out Connector con1, out Connector con2,
                    connectors1, connectors2);

                    familyInstance = Doc.Create.NewElbowFitting(con1, con2);
                }

                catch (Exception e)
                { }
                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }
            return familyInstance;
        }


        /// <summary>
        /// Create elbow or tee by given connectors
        /// </summary>
        /// <param name="con1"></param>
        /// <param name="con2"></param>
        /// <param name="con3"></param>
        /// <returns></returns>
        public FamilyInstance CreateFittingByConnectors(Connector con1, Connector con2, Connector con3 = null)
        {
            FamilyInstance familyInstance = null;
            using (Transaction transNew = new Transaction(Doc, "CreateTeeByConnectors"))
            {
                try
                {
                    transNew.Start();
                    if (con3 is null)
                    {

                        familyInstance = Doc.Create.NewElbowFitting(con1, con2);
                    }
                    else
                    {
                        familyInstance = Doc.Create.NewTeeFitting(con1, con2, con3);
                    }
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

            return familyInstance;
        }

        /// <summary>
        /// Create takeoff fitting
        /// </summary>
        /// <param name="con"></param>
        /// <param name="mEPCurve"></param>
        /// <returns></returns>
        public FamilyInstance CreateTakeOffFitting(Connector con, MEPCurve mEPCurve)
        {
            FamilyInstance familyInstance = null;
            using (Transaction transNew = new Transaction(Doc, "CreateTakeOff"))
            {
                try
                {
                    transNew.Start();
                    familyInstance = Doc.Create.NewTakeoffFitting(con, mEPCurve);
                }

                catch (Exception e)
                {
                    //transNew.RollBack();
                    //TaskDialog.Show("Revit", e.ToString());
                }
                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return familyInstance;
        }


        public FamilyInstance CreateFamilyInstane(FamilySymbol familySymbol)
        {
            FamilyInstance familyInstance = null;
            using (Transaction transNew = new Transaction(Doc, "CreateFamInst"))
            {
                try
                {
                    transNew.Start();

                    familyInstance = Doc.Create.NewFamilyInstance(new XYZ(0,0,0), familySymbol, MEPLevel, 
                        Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                }

                catch (Exception e)
                { }
                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return familyInstance;
        }

    }
}

