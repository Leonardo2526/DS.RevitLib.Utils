using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="BoundingBoxXYZ"/>.
    /// </summary>
    public static class BoxXYZExtensions
    {
        /// <summary>
        /// Show <paramref name="boxXYZ"/> in document as outline model lines.
        /// </summary>
        /// <param name="boxXYZ"></param>
        /// <param name="doc"></param>
        public static void Show(this BoundingBoxXYZ boxXYZ, Document doc) => 
            new BoundingBoxVisualisator(boxXYZ, doc).Visualise();

        /// <summary>
        /// Get minimum and maximum coordinate points from <paramref name="boxXYZ"/> in Revit coordinates. 
        /// </summary>
        /// <param name="boxXYZ"></param>
        /// <returns>
        /// Transformed min and max points of <paramref name="boxXYZ"/> that were defined in the coordinate space of the box.
        /// </returns>
        public static (XYZ minPoint, XYZ maxPoint) GetMinMaxPoints(this BoundingBoxXYZ boxXYZ)
        {
            var transform = boxXYZ.Transform;
            XYZ minPoint = boxXYZ.Min;
            XYZ maxPoint = boxXYZ.Max;

            return (transform.OfPoint(minPoint), transform.OfPoint(maxPoint));
        }
        
    }
}
