using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Geometry;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Face"/>.
    /// </summary>
    /// 
    public static class FaceExtensions
    {
        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.Face"/> from <paramref name="faceArray"/> by <paramref name="point"/> on it.
        /// </summary>
        /// <param name="faceArray"></param>
        /// <param name="point"></param>
        /// <returns>Returns first <see cref="Autodesk.Revit.DB.Face"/> from <paramref name="faceArray"/> if it contains <paramref name="point"/>.
        /// <para>
        /// Otherwise returns <see langword="null"></see>.
        /// </para>
        /// </returns>
        public static Face GetFace(this FaceArray faceArray, XYZ point)
        {
            for (int i = 0; i < faceArray.Size; i++)
            {
                Face face = faceArray.get_Item(i);
                var projPoint = face.Project(point).XYZPoint;
                if ((projPoint - point).IsZeroLength())
                {
                    return face;
                }
            }
            return null;
        }


        /// <summary>
        /// Get <paramref name="face"/>'s edges.
        /// </summary>
        /// <param name="face"></param>
        public static IEnumerable<Curve> GetEdges(this Face face)
        {
            var curves = new List<Curve>();

            var curveLoops = face.GetEdgesAsCurveLoops();
            foreach (var curveLoop in curveLoops)
            { curveLoop.ForEach(curves.Add); }

            return curves;
        }

        /// <summary>
        /// Show <paramref name="face"/>'s edges in <paramref name="doc"/>.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="doc"></param>
        public static void ShowEdges(this Face face, Document doc)
        => GetEdges(face).ToList().ForEach(c => c.Show(doc));

        /// <summary>
        /// Get list of <see cref="Rhino.Geometry.Point3d"/>s from <paramref name="face"/>.
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static IEnumerable<Rhino.Geometry.Point3d> Tesselate(this PlanarFace face)
        {
            var rect = Rectangle3dFactoty.Create(face);
            return new List<Rhino.Geometry.Point3d>()
            { rect.Corner(0), rect.Corner(1), rect.Corner(2), rect.Corner(3)};
        }

        /// <summary>
        /// Project <paramref name="point"/> on <paramref name="face"/> with ability to get projection on <paramref name="face"/>'s 
        /// closest edge if ordinary projection is <see langword="null"/>.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="point"></param>
        /// <param name="canProjectOnEdge"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.XYZ"/> if projection successful.
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static XYZ Project(this Face face, XYZ point, bool canProjectOnEdge)
        {
            XYZ projPoint = null;

            var result = face.Project(point);
            if (result != null)
            {
                projPoint = result.XYZPoint;
            }
            else if (canProjectOnEdge)
            {
                var fEdges = face.GetEdges().ToList();
                var closestEdge = fEdges.OrderByDescending(e => e.Distance(point)).Last();
                var eResult = closestEdge.Project(point);
                if (eResult != null)
                { projPoint = eResult.XYZPoint; }
            }

            return projPoint;
        }

        /// <summary>
        /// Project <paramref name="line"/> on <paramref name="face"/> with ability to get projection on 
        /// closest edge if ordinary projection is <see langword="null"/>.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="line"></param>
        /// <param name="canProjectOnEdge"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Line"/> if projection successful.
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static Line Project(this Face face, Line line, bool canProjectOnEdge)
        {
            Line projLine = null;

            var p1 = line.GetEndPoint(0);
            var p2 = line.GetEndPoint(1);
            XYZ p1Proj = Project(face, p1, canProjectOnEdge);
            if (p1Proj == null) { return null; }
            XYZ p2Proj = Project(face, p2, canProjectOnEdge);
            if (p2Proj == null) { return null; }

            if (p1Proj.DistanceTo(p2Proj) > 0.001)
            { projLine = Line.CreateBound(p1Proj, p2Proj); }

            return projLine;
        }
    }
}

