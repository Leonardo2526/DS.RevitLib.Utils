using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.Creator
{
    public class MEPSystemCreator
    {
        public MEPSystemCreator(Document doc, MEPCurve baseMEPCurve)
        {
            Doc = doc;
            BaseMEPCurve = baseMEPCurve;
        }


        #region Fields

        private readonly Document Doc;
        private readonly MEPCurve BaseMEPCurve;

        #endregion


        #region Properties

        public List<Element> MEPCurves { get; private set; } = new List<Element>();
        public List<Element> AllElements { get; private set; } = new List<Element>();
        public Connector StartConnector { get; private set; }
        public Connector EndConnector { get; private set; }

        #endregion


        #region Methods

        public List<Element> CreateSystem(List<XYZ> points)
        {
            MEPCurveCreator mEPCurveCreator = new MEPCurveCreator(Doc, BaseMEPCurve);
            FamInstCreator famInstCreator = new FamInstCreator(Doc);

            for (int i = 0; i < points.Count - 1; i++)
            {
                XYZ p1 = points[i];
                XYZ p2 = points[i + 1];

                MEPCurve mEPCurve = mEPCurveCreator.CreateMEPCurveByPoints(p1, p2);
                AllElements.Add(mEPCurve);
                MEPCurves.Add(mEPCurve);

                if (MEPCurves.Count > 1)
                {
                    FamilyInstance familyInstance = famInstCreator.CreateFittingByMEPCurves(
                        MEPCurves[i - 1] as MEPCurve, MEPCurves[i] as MEPCurve);
                    AllElements.Insert(AllElements.Count - 1, familyInstance);
                }
            }

            GetConnectors(AllElements);

            return AllElements;
        }

        private void GetConnectors(List<Element> elements)
        {
            (var free, var attach) = ConnectorUtils.GetConnectorsByAttach(elements.First());
            StartConnector = free;

            (free, attach) = ConnectorUtils.GetConnectorsByAttach(elements.Last());
            EndConnector = free;
        }

        #endregion
    }
}

