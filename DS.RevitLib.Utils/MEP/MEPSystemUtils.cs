using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public List<Element> Elements { get; private set; } = new List<Element>();


        public List<Element> CreateSystem(List<XYZ> points)
        {
            MEPSystemCreator mEPSystemCreator = new MEPSystemCreator(Doc, BaseMEPCurve);
            return mEPSystemCreator.CreateSystem(points);
        }

        public Element CreateFitting(MEPCurve mepCurve1, MEPCurve mepCurve2)
        {
            MEPSystemCreator mEPSystemCreator = new MEPSystemCreator(Doc, BaseMEPCurve);
            return mEPSystemCreator.CreateFittingByPipes(mepCurve1, mepCurve2);
        }

        public MEPCurve CreateMEPCurve(XYZ p1, XYZ p2)
        {
            MEPSystemCreator mEPSystemCreator = new MEPSystemCreator(Doc, BaseMEPCurve);
            return mEPSystemCreator.CreateMEPCurveByPoints(p1, p2);
        }
    }
}
