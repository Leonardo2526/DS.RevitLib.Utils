using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.GridMap;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Various;
using DS.RevitLib.Utils.Various.Selections;
using MoreLinq;
using MoreLinq.Extensions;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Search;
using Rhino.Geometry;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Extension methods for graph in Revit.
    /// </summary>
    public static class GraphExtensions
    {
        /// <summary>
        /// Try get <see cref="Edge{TVertex}"/> from <paramref name="graph"/>.
        /// <para>
        /// Specify <paramref name="edgeTag"/> to get it fast.
        /// </para>
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="location"></param>
        /// <param name="doc"></param>
        /// <param name="edgeTag"></param>
        /// <returns>
        /// <see cref="TaggedEdge{TVertex, TTag}"/> if it contains <paramref name="location"/> point.
        /// <para>
        /// <see langword="null"/> if no edges with specified <paramref name="edgeTag"/> or 
        /// <paramref name="location"/> point exists in <paramref name="graph"/>.
        /// </para>
        /// </returns>
        public static TaggedEdge<IVertex, int> TryGetEdge(this IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            Point3d location, Document doc, int edgeTag = -1)
        {
            var edges = graph.Edges.
                OfType<TaggedEdge<IVertex, int>>();
            if (edgeTag > 0)
            { edges = edges.Where(e => e.Tag == edgeTag); }

            if (edges.Count() == 0) { return null; }

            var xYZLocation = location.ToXYZ();
            foreach (var e in edges)
            {
                var l1 = e.Source.GetLocation(doc);
                var l2 = e.Target.GetLocation(doc);

                var line = Autodesk.Revit.DB.Line.CreateBound(l1, l2);
                if (line.Distance(xYZLocation) < 0.001)
                { return e; }
            }

            return null;
        }

        /// <summary>
        /// Try to insert <paramref name="pointOnMEPCurve"/> to <paramref name="graph"/>.
        /// <para>
        /// I.e. tries to find exist edge tag equals to <paramref name="mEPCurve"/>'s <see cref="Autodesk.Revit.DB.ElementId"/>
        /// and <paramref name="pointOnMEPCurve"/> between it's source and target vertices.  
        /// </para>
        /// <para>     
        /// If this edge was found replace it with two edges: (source-><paramref name="pointOnMEPCurve"/>) and (<paramref name="pointOnMEPCurve"/>->target).
        /// </para>     
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="pointOnMEPCurve"></param>
        /// <param name="mEPCurve"></param>
        /// <returns>
        /// <see langword="true"/> if edge to replace was found and insertion was successful.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool TryInsert(this AdjacencyGraph<IVertex,
            Edge<IVertex>> graph, MEPCurve mEPCurve, XYZ pointOnMEPCurve)
        {
            var doc = mEPCurve.Document;

            var location = pointOnMEPCurve.ToPoint3d();
            var tag = mEPCurve.Id.IntegerValue;
            var ap = new TaggedGVertex<Point3d>(graph.VertexCount, location);

            TaggedEdge<IVertex, int> edgeToRemove = graph.TryGetEdge(location, doc, tag);
            if (edgeToRemove == null) { return false; }

            var v0 = edgeToRemove.Source;
            var v1 = edgeToRemove.Target;
            graph.RemoveEdge(edgeToRemove);

            graph.AddVertex(ap);
            var edgeToAdd1 = new TaggedEdge<IVertex, int>(v0, ap, tag);
            var edgeToAdd2 = new TaggedEdge<IVertex, int>(ap, v1, tag);
            graph.AddEdge(edgeToAdd1);
            graph.AddEdge(edgeToAdd2);

            return true;
        }

        /// <summary>
        /// Try to insert <paramref name="point"/> into <paramref name="graph"/>.
        /// <para>
        /// I.e. try to find <paramref name="graph"/>'s edge that has line that contains given <paramref name="point"/>
        /// and try to split it on two edges and one auxiliary point.
        /// </para>
        /// <para>
        /// Specify <paramref name="edgeTag"/> to get it fast.
        /// </para>
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="point"></param>
        /// <param name="doc"></param>
        /// <param name="edgeTag"></param>
        /// <returns>
        /// <see langword="true"/> if insertion was successful.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool TryInsert(this AdjacencyGraph<IVertex,Edge<IVertex>> graph, Point3d point, Document doc, int edgeTag = -1)
        {
            var edge = graph.TryGetEdge(point, doc, edgeTag);
            var mc = edge?.TryGetMEPCurve(doc);

            return mc != null && graph.TryInsert(mc, point.ToXYZ());
        }

        /// <summary>
        /// Get path of <paramref name="graph"/> from <paramref name="root"/> to <paramref name="target"/> 
        /// and return it's length and verices count.
        /// <para>
        /// Get first <paramref name="graph"/>'s root if it isn't specified.
        /// In this case throw exeption if <paramref name="graph"/> roots count is not equal to 0.
        /// </para>
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="target"></param>
        /// <param name="doc"></param>
        /// <param name="root"></param>
        /// <returns>
        /// Sum of edges lengths and vertices count.
        /// <para>
        /// (0, 0) if path is <see langword="null"/> or it's count is 0;
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        public static (double length, int verticesCount) GetLengthToRoot(this IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            IVertex target, Document doc, IVertex root = null)
        {
            root ??= GetRoot();

            double length = 0;
            int count = 0;

            var path = graph.GetPath(root, target);
            if (path == null) { return (length, count); }

            foreach (var e in path)
            {
                var l = e.GetLength(doc);
                length += l;
            }

            if (path is not null && path.Count() > 0)
            { count = path.Count() + 1; }

            return (length, count);

            IVertex GetRoot()
            {
                var roots = graph.Roots();
                if (roots.Count() != 1) { throw new ArgumentException(); }
                else { return roots.First(); }
            }
        }

        /// <summary>
        /// Trim <paramref name="graph"/> with <paramref name="trimCategories"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="doc"></param>
        /// <param name="trimCategories"></param>
        /// <returns>
        /// A new graph with sink vertices that are not of <paramref name="trimCategories"/>.
        /// <para>
        /// <paramref name="graph"/> if <paramref name="trimCategories"/> are <see langword="null"/> or empty.
        /// </para>
        /// </returns>
        public static AdjacencyGraph<IVertex, Edge<IVertex>> Trim(this AdjacencyGraph<IVertex, Edge<IVertex>> graph, Document doc,
            Dictionary<BuiltInCategory, List<PartType>> trimCategories)
        {
            if (trimCategories is null || trimCategories.Count == 0) { return graph; }

            var clonedGraph = graph.Clone();
            var root = graph.Roots().First();

            IVertex catVertex = null;
            BreadthFirstSearchAlgorithm<IVertex, Edge<IVertex>> bfsToFindCategory;

            CheckCategoryVertex(doc, trimCategories, clonedGraph);
            while (catVertex is not null)
            {
                clonedGraph.ClearOutEdges(catVertex);
                var bfsToRemoveVerices = new BreadthFirstSearchAlgorithm<IVertex, Edge<IVertex>>(clonedGraph);
                bfsToRemoveVerices.SetRootVertex(root);
                bfsToRemoveVerices.Compute();

                var blackVerices = bfsToRemoveVerices.VertexColors.Where(c => c.Value != GraphColor.Black).ToList();
                blackVerices.ForEach(v => clonedGraph.RemoveVertex(v.Key));

                catVertex = null;
                CheckCategoryVertex(doc, trimCategories, clonedGraph);
            }

            return clonedGraph;

            void CheckCategoryVertex(Document doc, Dictionary<BuiltInCategory,
                List<PartType>> trimCategories, AdjacencyGraph<IVertex, Edge<IVertex>> clonedGraph)
            {
                bfsToFindCategory = new BreadthFirstSearchAlgorithm<IVertex, Edge<IVertex>>(clonedGraph);
                bfsToFindCategory.SetRootVertex(root);
                bfsToFindCategory.FinishVertex += Bfs_FinishVertex;
                bfsToFindCategory.Compute();

                void Bfs_FinishVertex(IVertex vertex)
                {
                    if (vertex is TaggedGVertex<int> tagged)
                    {
                        var famInst = tagged.TryGetFamilyInstance(doc);
                        if (famInst.IsCategoryElement(trimCategories) && !clonedGraph.IsOutEdgesEmpty(vertex))
                        {
                            catVertex = tagged;
                            bfsToFindCategory.Abort();
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Sort vertices pairs by relation length of <see cref="TaggedGVertex{TTag}"/> from <paramref name="graph"/> root.
        /// <para>
        /// Vertices on <see cref="Autodesk.Revit.DB.FamilyInstance"/>'s 
        /// will have high priority with relation length less than <paramref name="maxRelLength"/>.
        /// </para>
        /// </summary>
        /// <param name="vertexPair"></param>
        /// <param name="graph"></param>
        /// <param name="doc"></param>
        /// <param name="maxRelLength"></param>
        /// <param name="sizeFactor"></param>
        /// <returns>
        /// Sorted list where vertices on <see cref="Autodesk.Revit.DB.FamilyInstance"/>'s have highest priority.
        /// </returns>
        public static IEnumerable<(IVertex, IVertex)> SortByTaggedLength(this IEnumerable<(IVertex, IVertex)> vertexPair,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, Document doc, double maxRelLength = 25, double sizeFactor = 1)
        {
            var dict = new Dictionary<(IVertex, IVertex), double>();

            foreach (var pair in vertexPair)
            {
                var rl1 = GetPriority(pair.Item1);
                var rl2 = GetPriority(pair.Item2);
                var sum = rl1 + rl2;
                dict.Add(pair, sum);
            }

            var result = dict.OrderBy(kv => kv.Value);
            return result.ToList().Select(k => k.Key);

            double GetPriority(IVertex vertex)
            {
                (double length, int verticesCount) = graph.GetLengthToRoot(vertex, doc);
                switch (vertex)
                {
                    case TaggedGVertex<int> taggedInt:
                        {
                            var relLength = length / sizeFactor;
                            if (relLength < maxRelLength)
                            { length = 0; }
                            break;
                        }
                    default:
                        break;
                }

                return length;
            }
        }

        /// <summary>
        /// Get first edge occurence from <paramref name="graph"/> edges that contains given <paramref name="point"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="point"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see cref="TaggedEdge{TVertex, TTag}"/> if any edges of <paramref name="graph"/> contains <paramref name="point"/>.
        /// </returns>
        public static TaggedEdge<IVertex, int> GetEdge(this IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            Point3d point, Document doc)
        {
            var taggedEdges = graph.Edges.OfType<TaggedEdge<IVertex, int>>();
            var e = taggedEdges.FirstOrDefault();

            return taggedEdges.FirstOrDefault(e => e.Contains(point, doc));
        }

        /// <summary>
        /// Get <see cref=" Autodesk.Revit.DB.ElementId"/>s between <paramref name="source"/> and <paramref name="target"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see cref="MEPCurve"/>s from <see cref="Edge{TVertex}"/> and 
        /// <see cref="Autodesk.Revit.DB.FamilyInstance"/>s from <see cref="IVertex"/>s
        /// of <paramref name="graph"/>.
        /// <para>
        /// Empty list if no <see cref=" Autodesk.Revit.DB.ElementId"/>s exist between given <paramref name="source"/> and <paramref name="target"/>.
        /// </para>
        /// </returns>
        public static IEnumerable<ElementId> GetElementIds(this IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            IVertex source, IVertex target, Document doc)
        {
            var elements = new List<ElementId>();

            var edges = graph.GetPath(source, target);
            if (edges is null || edges.Count() == 0) { return elements; } 

            foreach (var edge in edges)
            {
                var e1 = edge.Source.TryGetFamilyInstance(doc);
                if (e1 != null) { elements.Add(e1.Id); }

                var mc = edge.TryGetMEPCurve(doc);
                if (mc != null) { elements.Add(mc.Id); }
            }
            var e2 = edges.Last().Target.TryGetFamilyInstance(doc);
            if (e2 != null) { elements.Add(e2.Id); }

            return elements;
        }
    }
}
