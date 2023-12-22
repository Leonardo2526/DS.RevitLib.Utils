using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using Rhino.Geometry;
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
        /// Create <see cref="Rectangle3d"/> from <paramref name="planarFace"/>'s boundingBox.
        /// </summary>
        /// <param name="planarFace"></param>
        /// <returns>
        /// A new <see cref="Rectangle3d"/>.
        /// </returns>
        public static Rectangle3d Create(PlanarFace planarFace)
        {
            var box = planarFace.GetBoundingBox();

            var p1 = planarFace.Evaluate(box.Min).ToPoint3d();
            var p2 = planarFace.Evaluate(box.Max).ToPoint3d();
            var plane = new Rhino.Geometry.Plane(
                planarFace.Origin.ToPoint3d(), 
                planarFace.FaceNormal.ToVector3d()
                );
           return new Rectangle3d(plane, p1, p2);
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

    }
}
