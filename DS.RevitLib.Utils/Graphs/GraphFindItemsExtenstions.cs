using Autodesk.Revit.DB;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Extensions;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Extension methods to find item on graph.
    /// </summary>
    public static class GraphFindItemsExtenstions
    {
        private static Func<IVertex, Point3d> GetLocation(Document doc)
        => (v) => v.GetLocation(doc).ToPoint3d();

        /// <summary>
        /// Try to find <see cref="IVertex"/> on <paramref name="point"/> in <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="point"></param>
        /// <param name="doc"></param>
        /// <param name="tolerance"></param>
        /// <param name="vertex"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="graph"/> has <see cref="IVertex"/> on specified <see cref="Point3d"/>.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool TryFindVertex(this IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            Point3d point, Document doc,
            out IVertex vertex, int tolerance = 3)
            => graph.TryFindVertex(point, GetLocation(doc),
                out vertex, tolerance);

        /// <summary>
        /// Try to find <see cref="IVertex"/> with <paramref name="item"/> in <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="item"></param>
        /// <param name="doc"></param>
        /// <param name="vertex"></param>
        /// <param name="tolerance"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="graph"/> has <see cref="IVertex"/> 
        /// on specified <see cref="Point3d"/> with tag equals to <paramref name="item"/> <see cref="int"/> value.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool TryFindVertex(this IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            (int value, Point3d point) item, Document doc,
            out IVertex vertex, int tolerance = 3)
           => graph.TryFindVertex(item, GetLocation(doc), out vertex, tolerance);

        /// <summary>
        /// Try to find <see cref="IVertex"/> and <see cref="TaggedEdge{TVertex, TTag}"/> with <paramref name="item"/> in <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="item"></param>
        /// <param name="doc"></param>
        /// <param name="vertex"></param>
        /// <param name="edges"></param>
        /// <param name="tolerance"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="graph"/> has  <see cref="IVertex"/> or <see cref="TaggedEdge{TVertex, TTag}"/> 
        /// with <paramref name="item"/> <see cref="int"/> value that contains <paramref name="item"/> <see cref="Point3d"/>.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool TryFindItems(this IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            (int value, Point3d point) item, Document doc,
            out IVertex vertex,
            out IEnumerable<TaggedEdge<IVertex, int>> edges,
            int tolerance = 3)
            => graph.TryFindItems(item, GetLocation(doc), out vertex, out edges, tolerance);
    }
}
