using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using QuickGraph;
using Rhino.Geometry;
using Line = Rhino.Geometry.Line;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Extension methods for <see cref="Edge{TVertex}"/>.
    /// </summary>
    public static class EdgeExtensions
    {
        /// <summary>
        /// Get <paramref name="edge"/>'s length.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// Distance between <paramref name="edge"/>'s source and target location points.
        /// </returns>
        public static double GetLength(this IEdge<IVertex> edge, Document doc)
            => GetLocationLine(edge, doc).Length;        

        /// <summary>
        /// Get <paramref name="edge"/>'s <see cref="Line"/>.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see cref="Line"/> built from <paramref name="edge"/>'s source to target locations.
        /// </returns>
        public static Line GetLocationLine(this IEdge<IVertex> edge, Document doc)
        {
            var loc1 = edge.Source.GetLocation(doc).ToPoint3d();
            var loc2 = edge.Target.GetLocation(doc).ToPoint3d();

            return new Line(loc1, loc2);
        }

        /// <summary>
        /// Get <paramref name="edge"/>'s direcion.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// Normilized <see cref="Vector3d"/> that specifies <paramref name="edge"/>'s line direction.
        /// </returns>
        public static Vector3d Direction(this IEdge<IVertex> edge, Document doc)
        {
            var dir= GetLocationLine(edge, doc).Direction;
            return Vector3d.Divide(dir, dir.Length); 
        }        

        /// <summary>
        /// Try get <see cref="MEPCurve"/> from <paramref name="edge"/>.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see cref="MEPCurve"/> if <paramref name="edge"/> is <see cref="TaggedEdge{TVertex, TTag}"/> 
        /// and it's possible to get element from <see cref="Document"/> by tagId.
        /// <para>
        /// Otherwise <see langword="null"/>.
        /// </para>
        /// </returns>
        public static MEPCurve TryGetMEPCurve(this IEdge<IVertex> edge, Document doc) =>
            edge is TaggedEdge<IVertex, int> taggedEdge ?
            doc.GetElement(new ElementId(taggedEdge.Tag)) as MEPCurve :
            null;

        /// <summary>
        /// Specifies if <paramref name="point"/> is between <paramref name="edge"/>'s source and target location 
        /// with specified <paramref name="tolerance"/>.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="point"></param>
        /// <param name="doc"></param>
        /// <param name="tolerance"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="point"/> lies on <paramref name="edge"/> or 
        /// coinsident with <paramref name="edge"/>'s end points.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool Contains(this IEdge<IVertex> edge, Point3d point, Document doc, double tolerance = 0.001)
        {
            var loc1 = edge.Source.GetLocation(doc).ToPoint3d();
            var loc2 = edge.Target.GetLocation(doc).ToPoint3d();

            var line = new Rhino.Geometry.Line(loc1, loc2);

            return line.Contains(point, tolerance);
        }

        /// <summary>
        /// Get location of <paramref name="edge"/>'s <see cref="Autodesk.Revit.DB.Connector"/>'s
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Connector"/>'s locations on <paramref name="edge"/>'s source and target.
        /// <para>
        /// Location <see cref="Point3d"/> if no connectors exists.
        /// </para>
        /// <para>
        /// <see cref="Point3d"/> default value if it was failed to get source or target location.
        /// </para>
        /// </returns>
        public static (Point3d source, Point3d target) GetConnectorsLocation(this IEdge<IVertex> edge, Document doc)
        {
            Point3d p1;
            Point3d p2;

            var sourceFamInst = edge.Source.TryGetFamilyInstance(doc);
            var targetFamInst = edge.Target.TryGetFamilyInstance(doc);

            var taggedEdge = edge as TaggedEdge<IVertex, int>;
            var mc = taggedEdge?.TryGetMEPCurve(doc);

            var c1 = sourceFamInst is null ?
                edge.Source.GetLocation(doc) :
                ConnectorUtils.GetCommonConnectors(mc, sourceFamInst).elem1Con.Origin;
            var c2 = targetFamInst is null ?
                edge.Target.GetLocation(doc) :
                ConnectorUtils.GetCommonConnectors(mc, targetFamInst).elem1Con.Origin;

            p1 = c1 is not null ? c1.ToPoint3d() : default;
            p2 = c2 is not null ? c2.ToPoint3d() : default;

            return (p1, p2);
        }

        /// <summary>
        /// Get segment from <paramref name="edge"/> that specifies <see cref="Line"/> reduced from 
        /// <paramref name="edge"/>'s <see cref="Autodesk.Revit.DB.Connector"/>s by offsets.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="doc"></param>
        /// <param name="offsetFromSourceCon"></param>
        /// <param name="offsetFromTargetCon"></param>
        /// <returns>
        /// <see cref="Line"/> if it was able to get source/target <see cref="Autodesk.Revit.DB.Connector"/>s or veritces location.
        /// <para>
        /// Otherwise <see cref="Line"/>'s default value.
        /// </para>
        /// </returns>
        public static Line GetConnectionSegment(this IEdge<IVertex> edge, Document doc,
            double offsetFromSourceCon, double offsetFromTargetCon)
        {
            Line line = default;

            var (source, target) = edge.GetConnectorsLocation(doc);
            var initLine = new Line(source, target);
            if (initLine.Extend(-offsetFromSourceCon, -offsetFromTargetCon))
            { line = initLine; }

            return line;
        }

    }
}
