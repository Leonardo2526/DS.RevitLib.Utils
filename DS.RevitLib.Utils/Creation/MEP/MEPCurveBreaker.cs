using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP;
using System.Linq;
using System.Security.Cryptography;

namespace DS.RevitLib.Utils.Creation.MEP
{
    /// <summary>
    /// An object to break any MEPCurve.
    /// </summary>
    public static class MEPCurveBreaker
    {
        /// <summary>
        /// Break <paramref name="mEPCurve"/> at <paramref name="point"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="mEPCurve">MEPCurve to break.</param>
        /// <param name="point">Break point.</param>
        /// <param name="connect"></param>
        /// <returns>Returns a new created MEPCurve.</returns>
        public static MEPCurve Break(MEPCurve mEPCurve, XYZ point, bool connect = false)
        {
            Document doc = mEPCurve.Document;

            //copy mepCurveToOptimize as newPipe and move to brkPoint
            var location = mEPCurve.Location as LocationCurve;
            var start = location.Curve.GetEndPoint(0);
            var end = location.Curve.GetEndPoint(1);
            var copiedEls = ElementTransformUtils.CopyElement(doc, mEPCurve.Id, point - start);
            var newId = copiedEls.First();

            //shorten mepCurveToOptimize and newPipe (adjust endpoints)
            AdjustMepCurve(mEPCurve, start, point);
            var newMEPCurve = doc.GetElement(newId) as MEPCurve;
            AdjustMepCurve(newMEPCurve, point, end);

            if (connect) 
            {
                doc.Regenerate();
                MEPElementUtils.Connect(mEPCurve, newMEPCurve); 
            }

            return newMEPCurve;
        }

        private static void AdjustMepCurve(MEPCurve mepCurve, XYZ p1, XYZ p2)
        {          
            var location = mepCurve.Location as LocationCurve;
            location.Curve = Line.CreateBound(p1, p2);
        }
    }
}
