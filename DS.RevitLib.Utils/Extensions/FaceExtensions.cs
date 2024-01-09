using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
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
            if(!Rectangle3dFactoty.TryCreate(face, out var rect)) { throw new Exception(); }
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

        /// <summary>
        /// Project <paramref name="rectangle"/> on <paramref name="face"/> with ability to get projection on 
        /// closest edge if ordinary projection is <see langword="null"/>.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="rectangle"></param>
        /// <param name="canProjectOnEdge"></param>
        /// <param name="projRectangle"></param>
        /// <returns>
        /// <see langword="true"/> if projection successful.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool TryProject(this Face face, Rhino.Geometry.Rectangle3d rectangle, 
            bool canProjectOnEdge, out Rhino.Geometry.Rectangle3d projRectangle)
        {
            projRectangle = default;
            var lines = GeometryElementsUtils.ToRevitLines(rectangle.ToLines());

            //get projections
            var projectEdgesValueResult = new List<Line>();
            foreach (var edge in lines)
            {
                var proj = face.Project(edge, canProjectOnEdge);
                if (proj != null)
                { projectEdgesValueResult.Add(proj); }
            }
            var rhinoEdges = GeometryElementsUtils.ToRhinoLines(projectEdgesValueResult);

            return GeometryUtils.TryCreateRectangle(rhinoEdges, out projRectangle);
        }

        /// <summary>
        /// Get outer boundary of <paramref name="face"/>.
        /// </summary>
        /// <param name="face"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.CurveLoop"/> with max length.
        /// <para>
        /// <see langword="null"/> if failed to get loops. 
        /// </para>
        /// </returns>
        public static CurveLoop GetOuterLoop(this Face face)
            => face.GetEdgesAsCurveLoops()?.
            OrderByDescending(curveLoops => curveLoops.GetExactLength()).
            First();

        /// <summary>
        /// Check if <paramref name="planarFace"/> contains <paramref name="line"/>.
        /// </summary>
        /// <param name="planarFace"></param>
        /// <param name="line"></param>
        /// <param name="tolerance"></param>
        /// <returns>
        /// <see langword="true"/> if both <paramref name="line"/>'s ends lie on <paramref name="planarFace"/>;
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool Contains(this PlanarFace planarFace, Line line, int tolerance = 3)
        {
            double t = Math.Pow(0.1, tolerance);

            var p1 = line.GetEndPoint(0);
            var p2 = line.GetEndPoint(1);

            var pjP1 = planarFace.Project(p1);
            if(pjP1 is null) { return false; }
            var pjP2 = planarFace.Project(p2);
            if (pjP2 is null) { return false; }

            return 
                pjP1.XYZPoint.DistanceTo(p1) < t 
                && pjP2.XYZPoint.DistanceTo(p2) < t;
        }
    }
}

