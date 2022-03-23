using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

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
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }
            return familyInstance;
        }

        /// <summary>
        /// Create fitting between two pipes
        /// </summary>
        /// <param name="mepCurve1"></param>
        /// <param name="mepCurve2"></param>
        public FamilyInstance CreateFittingByConnectors(Connector con1, Connector con2)
        {
            FamilyInstance familyInstance = null;
            using (Transaction transNew = new Transaction(Doc, "CreateFittingByConnectors"))
            {
                try
                {
                    transNew.Start();
                    familyInstance = Doc.Create.NewElbowFitting(con1, con2);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }

            return familyInstance;
        }

    }
}

