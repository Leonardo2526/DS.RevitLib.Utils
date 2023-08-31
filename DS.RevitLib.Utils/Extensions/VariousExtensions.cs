using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Creation.Transactions;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Extension methods for various objects.
    /// </summary>
    public static class VariousExtensions
    {
        /// <summary>
        /// Get <see cref=" Autodesk.Revit.DB.BoundingBoxIntersectsFilter"/> by <paramref name="outline"/> with dependency on <paramref name="link"/> type.
        /// </summary>
        /// <param name="outline"></param>
        /// <param name="link"></param>
        /// <returns>
        /// <see cref=" Autodesk.Revit.DB.BoundingBoxIntersectsFilter"/> by <paramref name="outline"/> or 
        /// transformed <paramref name="outline"/> if <paramref name="link"/> is <see cref="RevitLinkInstance"/>.
        /// <para>
        /// <see langword="null"/> if <paramref name="outline"/> is null.
        /// </para>
        /// </returns>
        public static BoundingBoxIntersectsFilter GetBoundingBoxFilter(this Outline outline, RevitLinkInstance link)
        {
            if (outline == null) { return null; }

            var tr = link.GetTotalTransform();
            var filterOutline = new Outline(tr.Inverse.OfPoint(outline.MinimumPoint), tr.Inverse.OfPoint(outline.MaximumPoint));

            (XYZ min, XYZ max) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { filterOutline.MinimumPoint, filterOutline.MaximumPoint });
            filterOutline.MinimumPoint = min; filterOutline.MaximumPoint = max;
            return new BoundingBoxIntersectsFilter(filterOutline); ;
        }

        /// <summary>
        /// Show <paramref name="outline"/> in <paramref name="doc"/> model.
        /// </summary>
        /// <param name="outline"></param>
        /// <param name="doc"></param>
        /// <param name="labelSize"></param>
        /// <param name="transactionFactory"></param>
        public static void Show(this Outline outline, Document doc, ITransactionFactory transactionFactory, double labelSize = 0)
        {
            var bb = new BoundingBoxXYZ
            {
                Min = outline.MinimumPoint,
                Max = outline.MaximumPoint
            };
            var points = bb.GetPoints();
            points.ForEach(p => p.Show(doc, labelSize, transactionFactory));
            transactionFactory.CreateAsync(() => bb.Show(doc), "showBoxXYZ");
        }

        /// <summary>
        /// Get intersection <see cref="Autodesk.Revit.DB.Outline"/> between <paramref name="outline1"/> and <paramref name="outline2"/>.
        /// </summary>
        /// <param name="outline1"></param>
        /// <param name="outline2"></param>
        /// <returns>
        /// A new <see cref="Autodesk.Revit.DB.Outline"/> with minPoint and maxPoint not less or more than <paramref name="outline2"/>'s minPoint and
        /// maxPoint relatively.
        /// <para>
        /// <paramref name="outline1"/> if <paramref name="outline2"/> contains it.
        /// </para>
        /// </returns>
        public static Outline GetIntersection(this Outline outline1, Outline outline2)
        {
            if (outline2.ContainsOtherOutline(outline1, 0))
            { return outline1; }

            var boundingBox1 = new BoundingBox(outline1.MinimumPoint.ToPoint3d(), outline1.MaximumPoint.ToPoint3d());
            var boundingBox2 = new BoundingBox(outline2.MinimumPoint.ToPoint3d(), outline2.MaximumPoint.ToPoint3d());

            var intersectionBox = BoundingBox.Intersection(boundingBox1, boundingBox2);

            return new Outline(intersectionBox.Min.ToXYZ(), intersectionBox.Max.ToXYZ());
        }

    }
}
