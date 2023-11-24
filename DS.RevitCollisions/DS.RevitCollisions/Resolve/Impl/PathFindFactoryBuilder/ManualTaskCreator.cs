using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.GridMap;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using QuickGraph;
using Rhino.Geometry;
using Rhino.UI;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace DS.RevitCollisions.Resolve.Impl.PathFindFactoryBuilder
{
    /// <summary>
    /// An object for manual create tasks to resolve collision.
    /// </summary>
    public class ManualTaskCreator : ITaskCreator<IMEPCollision, (IVertex, IVertex)>
    {
        private readonly IVertexPointer _pointer;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private readonly Document _doc;

        /// <summary>
        /// Instantiate an object for manual create tasks to resolve collision.
        /// </summary>
        /// <param name="collision"></param>
        /// <param name="pointStrategy"></param>
        public ManualTaskCreator(IVertexPointer pointStrategy,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, Document doc)
        {
            _pointer = pointStrategy;
            _graph = graph;
            _doc = doc;
        }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        public (IVertex, IVertex) CreateTask(IMEPCollision item)
        {
            var v1 = _pointer.Point("1");
            if (v1 == null) { return (null, null); }

            var v2 = _pointer.Point("2");
            if (v2 == null) { return (null, null); }

            var graphVertex1 = TryGetGraphVertex(v1);
            var graphVertex2 = TryGetGraphVertex(v2);

            return (graphVertex1, graphVertex2);
        }

        private IVertex TryGetGraphVertex(IVertex initVertex)
        {
            if (!initVertex.TryFindTaggedVertex(_graph, out var foundVertex))
            {
                var aGraph = _graph as AdjacencyGraph<IVertex, Edge<IVertex>>;
                var location = initVertex.GetLocation(_doc).ToPoint3d();
                if (aGraph.TryInsert(location, _doc))
                { foundVertex = aGraph.Vertices.Last(); }
            }

            return foundVertex;
        }


    }
}
