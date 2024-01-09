using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Extensions;
using Rhino.Geometry;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Geometry
{
    /// <summary>
    /// Factory to create <see cref="Rectangle3d"/>.
    /// </summary>
    public static class Rectangle3dFactoty
    {
        /// <summary>
        /// Create outer boundary as <see cref="Rectangle3d"/> from <paramref name="face"/>.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="rectangle"></param>
        /// <returns>
        /// A new <see cref="Rectangle3d"/> or it's default value.
        /// </returns>
        public static bool TryCreate(PlanarFace face, out Rectangle3d rectangle)
        {
            rectangle = default;
            var outerLoop = face.GetOuterLoop().Select(x => x).ToList();
            return outerLoop != null && TryCreate(outerLoop, out rectangle);
        }

        /// <summary>
        /// Create <see cref="Rectangle3d"/> from <paramref name="curves"/>.
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="rectangle"></param>
        /// <returns>
        /// A new <see cref="Rectangle3d"/>.
        /// </returns>
        public static bool TryCreate(IEnumerable<Autodesk.Revit.DB.Curve> curves, out Rectangle3d rectangle)
        {
            rectangle = default;
            var lines = new List<Rhino.Geometry.Line>();
            curves.ToList().ForEach(c => lines.Add(
                new Rhino.Geometry.Line(c.GetEndPoint(0).ToPoint3d(), c.GetEndPoint(1).ToPoint3d())
                ));

            return GeometryUtils.TryCreateRectangle(lines, out rectangle);
        }

        /// <summary>
        /// Create <see cref="Rectangle3d"/> from <paramref name="curveLoop"/> on <paramref name="plane"/>.
        /// </summary>       
        /// <param name="curveLoop"></param>
        /// <param name="rectangle"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="rectangle"/> was created successfully.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool TryCreate(CurveLoop curveLoop, out Rectangle3d rectangle)
        {
            rectangle = default;
            if (curveLoop.IsOpen()) { return false; }
            var curves = curveLoop.ToList();
            return TryCreate(curves, out rectangle);
        }

    }
}
