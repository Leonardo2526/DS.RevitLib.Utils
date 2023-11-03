using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Graphs;
using DS.ClassLib.VarUtils.GridMap;
using DS.RevitLib.Utils.Graphs;
using NUnit.Framework;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEPSystemGraphFactoryUnitTests
{
    internal class GraphTester
    {
        public static void IsValidSimpleChainOrTreeItemsCount(AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
            var b = graph.VertexCount - graph.EdgeCount == 1;

            Assert.Greater(graph.VertexCount, 0);
            Assert.Greater(graph.EdgeCount, 0);
            Assert.AreEqual(true, b);
        }

        public static void IsSpecificVerticesCount(AdjacencyGraph<IVertex, Edge<IVertex>> graph, int verticesCount)
        {
            Assert.Greater(graph.VertexCount, 0);
            Assert.AreEqual(verticesCount, graph.VertexCount);
        }

        public static void IsSpecificEdgesCount(AdjacencyGraph<IVertex, Edge<IVertex>> graph, int edgesCount)
        {
            Assert.Greater(graph.EdgeCount, 0);
            Assert.AreEqual(edgesCount, graph.EdgeCount);
        }

        public static void IsSpecificTaggedVerticesCount(AdjacencyGraph<IVertex, Edge<IVertex>> graph, int verticesCount)
        {
            var taggedVertices = graph.Vertices.OfType<TaggedGVertex<int>>();
            Assert.Greater(taggedVertices.Count(), 0);
            Assert.AreEqual(verticesCount, taggedVertices.Count());
        }

        public static void IsSpecificTaggedEdgesCount(AdjacencyGraph<IVertex, Edge<IVertex>> graph, int edgesCount)
        {
            var taggedEdges = graph.Vertices.OfType<TaggedEdge<IVertex, int>>();
            Assert.Greater(taggedEdges.Count(), 0);
            Assert.AreEqual(edgesCount, taggedEdges.Count());
        }

        public static void IsSimpleCycle(AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
            Assert.Greater(graph.VertexCount, 0);
            Assert.Greater(graph.EdgeCount, 0);
            Assert.AreEqual(graph.VertexCount, graph.EdgeCount);
        }

        /// <summary>
        /// Graph that hasn't duplicate verices or edges.
        /// </summary>
        /// <param name="graph"></param>
        public static void IsSimple(AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
            //check duplicate vertices
            var vertices= graph.Vertices;
            var pointVertices = graph.Vertices.OfType<TaggedGVertex<Point3d>>();         
            var duplicateVertices = pointVertices.
                GroupBy(v => v.Tag).
                Where(t => t.Count() > 1).
                SelectMany(g => g);

            Assert.Greater(vertices.Count(), 0);
            Assert.AreEqual(duplicateVertices.Count(), 0);

            //check duplicate tagged vertices
            var taggedVertices = graph.Vertices.OfType<TaggedGVertex<int>>();
            if (taggedVertices.Count() > 0)
            {
                var distinctTaggedVericies = taggedVertices.Distinct(new CompareGTaggedVertex()).ToList();
                var duplicateTaggedVertices = taggedVertices.Where(v => !distinctTaggedVericies.Contains(v)).ToList();
                Assert.AreEqual(duplicateTaggedVertices.Count, 0);
            }

            //check duplicate edges
            var taggedEdges = graph.Edges.OfType<TaggedEdge<IVertex, int>>();
            var distinctsEdges = taggedEdges.Distinct(new CompareGTaggedEdge()).ToList();
            var duplicateEdges = taggedEdges.Where(e => !distinctsEdges.Contains(e)).ToList();
            Assert.Greater(taggedEdges.Count(), 0);
            Assert.AreEqual(duplicateEdges.Count, 0);
        }

        public static void IsEmptyVertices(AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
            Assert.IsTrue(graph.VertexCount == 0);
        }

        public static void IsFailEmptyVertices(AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
            Assert.IsFalse(graph.VertexCount == 0);
        }
    }
}
