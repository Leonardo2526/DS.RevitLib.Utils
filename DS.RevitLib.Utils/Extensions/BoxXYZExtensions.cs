using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        
    }
}
