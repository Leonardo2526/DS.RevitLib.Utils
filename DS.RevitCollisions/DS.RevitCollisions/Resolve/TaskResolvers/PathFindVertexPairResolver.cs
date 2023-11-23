using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.PathFinder;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.PathCreators;
using QuickGraph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using System.Linq;
using Rhino.Geometry;
using QuickGraph.Algorithms;
using DS.RevitLib.Utils.Graphs;

namespace DS.RevitCollisions
{
    internal class PathFindVertexPairResolver : ITaskResolver<(IVertex, IVertex), IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>>
    {
        private readonly XYZVertexPathFinder _pathFinder;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private readonly Document _doc;
        private readonly IMEPCollision _mEPCollision;
        private readonly IElementCollisionDetector _collisionDetector;
        private readonly List<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> _results = new();

        public PathFindVertexPairResolver(
            XYZVertexPathFinder pathFinder,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            Document doc,
            IMEPCollision mEPCollision,
            IElementCollisionDetector collisionDetector)
        {
            _pathFinder = pathFinder;
            _graph = graph;
            _doc = doc;
            _mEPCollision = mEPCollision;
            _collisionDetector = collisionDetector;
        }

        public IEnumerable<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> Results => _results;

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }


        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> TryResolve((IVertex, IVertex) task)
        {
            BuildPathFinderWithTask(_pathFinder, _graph, _mEPCollision, task, _collisionDetector);
            var result = _pathFinder.FindPath(task.Item1, task.Item2);
            return GetResult(result);
        }


        public async Task<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> TryResolveAsync((IVertex, IVertex) task)
        {
            BuildPathFinderWithTask(_pathFinder, _graph, _mEPCollision, task, _collisionDetector);
            var result = await _pathFinder.FindPathAsync(task.Item1, task.Item2);
            return GetResult(result);
        }

        private IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> GetResult(List<XYZ> pathPoints)
        {

            if (pathPoints == null || pathPoints.Count == 0)
            {
                Logger?.Warning("Failed to find path.");
                return null;
            }

            Logger?.Information("Path with length " + pathPoints.Count + " points was found.");

            var resultPoints = new List<Point3d>();
            pathPoints.ForEach(r => resultPoints.Add(r.ToPoint3d()));

            var graph = AdjancyGraphUtils.CreateSimpleChainGraph(resultPoints);
            _results.Add(graph);

            return graph;
        }

        private XYZVertexPathFinder BuildPathFinderWithTask(XYZVertexPathFinder pathFinder,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            IMEPCollision mEPCollision, (IVertex, IVertex) task, IElementCollisionDetector collisionDetector)
        {
            var baseMEPCurve = mEPCollision.Item1;
            var stateMEPCurve = mEPCollision.Item2 as MEPCurve;

            List<Element> objectsToExclude = GetElementsToExclude(graph, task);

            pathFinder.Build(graph,
                baseMEPCurve,
                baseMEPCurve,
                stateMEPCurve,
                objectsToExclude, collisionDetector);

            return pathFinder;
        }

        /// <summary>
        /// Get elements to exclude from collisions.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        private List<Element> GetElementsToExclude(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, (IVertex, IVertex) task)
        {
            var objectsToExclude = new List<Element>();

            var idsToExclude = new List<ElementId>();
            var root = graph.Roots().First();
            var idsToExclude1 = graph.GetElementIds(root, task.Item1, _doc);
            var idsToExclude2 = graph.GetElementIds(root, task.Item2, _doc);
            idsToExclude.AddRange(idsToExclude1);
            idsToExclude.AddRange(idsToExclude2);

            //add outElements
            graph.TryGetOutEdges(task.Item1, out var e1);
            idsToExclude.AddRange(GetOutIds(e1));
            graph.TryGetOutEdges(task.Item2, out var e2);
            idsToExclude.AddRange(GetOutIds(e2));

            idsToExclude = idsToExclude.Distinct().ToList();
            idsToExclude.ForEach(id => objectsToExclude.Add(_doc.GetElement(id)));

            return objectsToExclude;

            IEnumerable<ElementId> GetOutIds(IEnumerable<IEdge<IVertex>> outEdges)
            {
                var excludeIds= new List<ElementId>();

                foreach (var item in outEdges)
                {
                    var mc = item.TryGetMEPCurve(_doc);
                    if (mc != null) { excludeIds.Add(mc.Id); }
                    else
                    {
                        var famInst = item.Target.TryGetFamilyInstance(_doc);
                        if (famInst != null)
                        { excludeIds.Add(famInst.Id); }
                    }
                }

                return excludeIds;
            }
        }
    }
}
