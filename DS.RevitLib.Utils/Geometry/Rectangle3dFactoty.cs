using Autodesk.Revit.DB;
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
        /// Create <see cref="Rectangle3d"/> from <paramref name="face"/>.
        /// </summary>
        /// <param name="face"></param>
        /// <returns>
        /// A new <see cref="Rectangle3d"/> or it's default value.
        /// </returns>
        public static Rectangle3d Create(PlanarFace face)
        {
            var loops = face.GetEdgesAsCurveLoops();
            if (loops != null && loops.Count == 1 && !loops[0].IsOpen())
            {
                if (TryCreate(loops[0], out var rectangle))
                { return rectangle; }
            }

            return default;
        }

        /// <summary>
        /// Create <see cref="Rectangle3d"/> from <paramref name="curves"/>.
        /// </summary>
        /// <param name="curves"></param>
        /// <returns>
        /// A new <see cref="Rectangle3d"/>.
        /// </returns>
        public static Rectangle3d Create(List<Autodesk.Revit.DB.Curve> curves)
        {
            if (curves.Count != 4) { throw new Exception(); }

            var lines = new List<Rhino.Geometry.Line>();
            curves.ForEach(c => lines.Add(
                new Rhino.Geometry.Line(c.GetEndPoint(0).ToPoint3d(), c.GetEndPoint(1).ToPoint3d())
                ));

            //create plane
            var origin = lines[0].From;

            var ordered = lines.OrderByDescending(l => l.Length);
            var xLength = lines[0].Length;

            var xDir = lines[0].To - lines[0].From;
            xDir.Unitize();

            lines.RemoveAt(0);
            var oLine = lines.FirstOrDefault(l => l.From.DistanceTo(origin) < 0.001 || l.To.DistanceTo(origin) < 0.001);
            var yDir = oLine.To - origin;
            yDir = yDir.Length < 0.001 ? oLine.From - origin : yDir;
            yDir.Unitize();
            var yLength = oLine.Length;
            var rgPlane = new Rhino.Geometry.Plane(origin, xDir, yDir);

            return new Rectangle3d(rgPlane, xLength, yLength);
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
            if(curveLoop.IsOpen()) { return false; }

            var curves = curveLoop.ToList();

            rectangle = Create(curves);
            return true;
        }

    }
}
