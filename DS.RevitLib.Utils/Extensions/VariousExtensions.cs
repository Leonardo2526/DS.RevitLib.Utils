using Autodesk.Revit.DB;
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

            return new BoundingBoxIntersectsFilter(filterOutline); ;
        }

    }
}
