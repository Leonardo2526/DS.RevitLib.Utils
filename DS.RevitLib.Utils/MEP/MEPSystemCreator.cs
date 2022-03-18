using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP
{
    class MEPSystemCreator
    {
        public MEPSystemCreator(Document doc, MEPCurve baseMEPCurve)
        {
            Doc = doc;
            BaseMEPCurve = baseMEPCurve;
        }

        #region Fields

        private readonly Document Doc;
        private readonly MEPCurve BaseMEPCurve;
        private readonly List<Element> MEPCurves = new List<Element>();
        private readonly List<Element> AllElements = new List<Element>();

        #endregion


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


        public List<Element> CreateSystem(List<XYZ> points)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                XYZ p1 = points[i];
                XYZ p2 = points[i + 1];

                CreateMEPCurveByPoints(p1, p2);

                if (MEPCurves.Count > 1)
                    CreateFittingByPipes(MEPCurves[i - 1] as MEPCurve, MEPCurves[i] as MEPCurve);
            }

            return AllElements;
        }

        /// <summary>
        /// Create pipe between 2 points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public MEPCurve CreateMEPCurveByPoints(XYZ p1, XYZ p2)
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

            AllElements.Add(mEPCurve);
            MEPCurves.Add(mEPCurve);
            return mEPCurve;
        }

        /// <summary>
        /// Create fitting between two pipes
        /// </summary>
        /// <param name="mepCurve1"></param>
        /// <param name="mepCurve2"></param>
        public Element CreateFittingByPipes(MEPCurve mepCurve1, MEPCurve mepCurve2)
        {
            FamilyInstance familyInstance = null;
            using (Transaction transNew = new Transaction(Doc, "CreateFittingByPipes"))
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

            AllElements.Add(familyInstance);
            return familyInstance;
        }

    }
}

