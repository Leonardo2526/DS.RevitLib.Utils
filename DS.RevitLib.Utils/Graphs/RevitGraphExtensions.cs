using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Graphs;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Various.Selections;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Extension methods for graph in Revit.
    /// </summary>
    public static class RevitGraphExtensions
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
        public static TaggedEdge<IVertex, int> TryGetEdge(this AdjacencyGraph<IVertex, Edge<IVertex>> graph,
            Point3d location, Document doc, int edgeTag = -1)
        {
            var edges = graph.Edges.
                OfType<TaggedEdge<IVertex, int>>();
            if (edgeTag > 0)
            { edges = edges.Where(e => e.Tag == edgeTag); }

            if(edges.Count() == 0) { return null; }

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
            if(edgeToRemove == null) { return false; }

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
    }
}
