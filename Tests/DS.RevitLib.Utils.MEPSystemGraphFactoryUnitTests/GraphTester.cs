using DS.ClassLib.VarUtils.Graphs;
using DS.ClassLib.VarUtils.GridMap;
using NUnit.Framework;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEPSystemGraphFactoryUnitTests
{
    internal class GraphTester
    {
        public static void IsValidSimpleChainOrTreeVertexCount(AdjacencyGraph<LVertex, Edge<LVertex>> graph)
        {
            var b = graph.VertexCount - graph.EdgeCount == 1;

            Assert.Greater(graph.VertexCount, 0);
            Assert.Greater(graph.EdgeCount, 0);
            Assert.AreEqual(true, b);
        }

        public static void IsEmptyVertices(AdjacencyGraph<LVertex, Edge<LVertex>> graph)
        {
            Assert.IsTrue(graph.VertexCount == 0);
        }

        public static void IsFailEmptyVertices(AdjacencyGraph<LVertex, Edge<LVertex>> graph)
        {
            Assert.IsFalse(graph.VertexCount == 0);
        }
    }
}
