using Autodesk.Revit.DB;
using DS.RevitLib.Utils.ModelCurveUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils
{
    /// <summary>
    /// Extension methods for 'Curve' object.
    /// </summary>
    public static class CurveExtensions
    {
        /// <summary>
        /// Extract plane by curve and point. 
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point">If point us null then point is assigned as one of the basis.</param>
        /// <returns></returns>
        public static Plane GetPlane(this Curve curve, XYZ point = null)
        {
            XYZ p1 = curve.GetEndPoint(0);
            XYZ p2 = curve.GetEndPoint(1);

            if (curve is Line)
            {
                point ??= XYZUtils.GenerateXYZ();
                return Plane.CreateByThreePoints(p1, p2, point);
            }
            else if (curve is Arc)
            {
                Arc arc = curve as Arc;
                return Plane.CreateByNormalAndOrigin(arc.Normal, arc.Center);
            }

            return null;
        }

        /// <summary>
        /// Create ModelCurve by given curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="doc"></param>
        /// <param name="point">Reference point to get curve plane.</param>
        public static void Show(this Curve curve, Document doc, XYZ point = null)
        {
            var creator = new ModelCurveCreator(doc);
            creator.Create(curve, point);
        }

        /// <summary>
        /// Get center of <paramref name="curve"/>.
        /// </summary>
        /// <param name="curve"></param>
        /// <returns>Returns center point of given line.</returns>
        public static XYZ GetCenter(this Curve curve)
        {
            XYZ startPoint = curve.GetEndPoint(0);
            XYZ endPoint = curve.GetEndPoint(1);
            XYZ centerPoint = new XYZ(
                (startPoint.X + endPoint.X) / 2,
                (startPoint.Y + endPoint.Y) / 2,
                (startPoint.Z + endPoint.Z) / 2);

            try
            {
                centerPoint = curve.Project(centerPoint)?.XYZPoint;
            }
            catch (Exception)
            { }

            return centerPoint;
        }
    }
}
