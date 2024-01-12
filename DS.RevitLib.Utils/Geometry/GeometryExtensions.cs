using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Extensions;
using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Lines;
using Plane = Autodesk.Revit.DB.Plane;

namespace DS.RevitLib.Utils.Geometry
{
    /// <summary>
    /// Extensions methods for geomtry primitives.
    /// </summary>
    public static class GeometryExtensions
    {
        /// <summary>
        /// Convert <paramref name="rectangle"/> to list of <see cref="Autodesk.Revit.DB.Line"/>.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns>
        /// <paramref name="rectangle"/> segments converted to list of <see cref="Autodesk.Revit.DB.Line"/>.
        /// </returns>
        public static List<Autodesk.Revit.DB.Line> ToRevitLines(this Rectangle3d rectangle)
        {
            var rlines = new List<Autodesk.Revit.DB.Line>();
            var rgLines = rectangle.ToLines();
            rgLines.ForEach(l => rlines.Add(Autodesk.Revit.DB.Line.CreateBound(l.From.ToXYZ(), l.To.ToXYZ())));
            return rlines;
        }

        /// <summary>
        /// Show <paramref name="rectangle"/> in <paramref name="doc"/>.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="doc"></param>
        public static void Show(this Rectangle3d rectangle, Document doc)
        {
            var rlines = ToRevitLines(rectangle);
            rlines.ForEach(obj => obj.Show(doc));
        }

        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.Plane"/> from <paramref name="planarFace"/>.
        /// </summary>
        /// <param name="planarFace"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Plane"/> created by <paramref name="planarFace"/>'s normal and origin.
        /// </returns>
        public static Plane GetPlane(this PlanarFace planarFace)
            => Plane.CreateByNormalAndOrigin(planarFace.FaceNormal, planarFace.Origin);

        /// <summary>
        /// Convert <paramref name="plane"/> to <see cref="Rhino.Geometry.Plane"/>.
        /// </summary>
        /// <param name="plane"></param>
        /// <returns>
        /// <see cref="Rhino.Geometry.Plane"/> created by <paramref name="plane"/>'s normal and origin.
        /// </returns>
        public static Rhino.Geometry.Plane ToRhinoPlane(this Plane plane)
            => new(plane.Origin.ToPoint3d(), plane.Normal.ToVector3d());

        /// <summary>
        /// Apply the <paramref name="transform"/> to the <paramref name="outline"/> and retun the result.
        /// </summary>
        /// <param name="outline"></param>
        /// <param name="transform"></param>
        /// <returns>
        /// The transformed <paramref name="outline"/>.
        /// </returns>
        public static Outline Transform(this Outline outline, Autodesk.Revit.DB.Transform transform)
        {
            var p1 = transform.OfPoint(outline.MinimumPoint);
            var p2 = transform.OfPoint(outline.MaximumPoint);
            (XYZ minPoint, XYZ maxPoint) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p1, p2 });
            return new Outline(minPoint, maxPoint);
        }
    }
}
