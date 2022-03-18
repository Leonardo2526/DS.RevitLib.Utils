using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP
{
    public class MEPSystemUtils
    {
        public MEPSystemUtils(Document doc, MEPCurve baseMEPCurve)
        {
            Doc = doc;
            BaseMEPCurve = baseMEPCurve;
        }

        #region Fields

        private readonly Document Doc;
        private readonly MEPCurve BaseMEPCurve;

        #endregion

        public Connector StartConnector { get; private set; }

        public Connector EndConnector { get; private set; }


        public List<Element> CreateSystem(List<XYZ> points)
        {
            MEPSystemCreator mEPSystemCreator = new MEPSystemCreator(Doc, BaseMEPCurve);
            List<Element> elements = mEPSystemCreator.CreateSystem(points);

            (var free, var attach) = ConnectorUtils.GetConnectorsByAttach(elements.First());
            StartConnector = free;

            (free, attach) = ConnectorUtils.GetConnectorsByAttach(elements.Last());
            EndConnector = free;

            return elements;
        }

        public Element CreateFittingByMEPCurves(MEPCurve mepCurve1, MEPCurve mepCurve2)
        {
            MEPSystemCreator mEPSystemCreator = new MEPSystemCreator(Doc, BaseMEPCurve);
            return mEPSystemCreator.CreateFittingByMEPCurves(mepCurve1, mepCurve2);
        }

        public Element CreateFittingByConnectors(Connector con1, Connector con2)
        {
            MEPSystemCreator mEPSystemCreator = new MEPSystemCreator(Doc, BaseMEPCurve);
            return mEPSystemCreator.CreateFittingByConnectors(con1, con2);
        }

        public MEPCurve CreateMEPCurve(XYZ p1, XYZ p2)
        {
            MEPSystemCreator mEPSystemCreator = new MEPSystemCreator(Doc, BaseMEPCurve);
            return mEPSystemCreator.CreateMEPCurveByPoints(p1, p2);
        }
    }
}
