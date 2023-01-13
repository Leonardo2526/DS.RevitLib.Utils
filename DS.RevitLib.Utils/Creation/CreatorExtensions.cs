using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Creation
{
    /// <summary>
    /// An object that represents extension methods for <see cref="Autodesk.Revit.Creation.Document"/>.
    /// </summary>
    public static class CreatorExtensions
    {
        /// <summary>
        /// Creates a new model line element.
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="doc"></param>
        /// <param name="curve"></param>
        /// <param name="point">Reference point to get curve plane.</param>
        /// <returns>If successful a new model line element. Otherwise <see langword="null"/>.</returns>
        public static ModelCurve NewModelCurve(this Autodesk.Revit.Creation.Document creator, Document doc, Curve curve, XYZ point = null)
        {
            Plane plane = curve.GetPlane(point);

            // Create a sketch plane in current document
            SketchPlane sketch = SketchPlane.Create(doc, plane);
           
            return creator.NewModelCurve(curve, sketch);
        }
    }
}
