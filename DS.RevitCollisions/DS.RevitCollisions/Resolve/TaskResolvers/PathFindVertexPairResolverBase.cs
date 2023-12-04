using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.GridMap;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.PathCreators;
using QuickGraph;
using QuickGraph.Algorithms;
using Rhino.Geometry;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DS.RevitCollisions.Resolve.TaskResolvers
{
    public abstract class PathFindVertexPairResolverBase :
        ITaskResolver<(IVertex, IVertex), IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>>
    {
        protected readonly IElementCollisionDetector _collisionDetector;
        protected readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        protected readonly Document _doc;
        protected readonly XYZPathFinder _pathFinder;
        private readonly List<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> _results = new();

        public PathFindVertexPairResolverBase(
           XYZPathFinder pathFinder,
           Document doc,
           IElementCollisionDetector collisionDetector,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            _pathFinder = pathFinder;
            _doc = doc;
            _collisionDetector = collisionDetector;
            _graph = graph;
        }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        public IEnumerable<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> Results => _results;


        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> TryResolve((IVertex, IVertex) task)
        {
            BuildPathFinderWithTask(_pathFinder, task, _collisionDetector);

            var c1 = ToConnectionPoint(task.Item1, _graph, _doc);
            var c2 = ToConnectionPoint(task.Item2, _graph, _doc);

            var result = _pathFinder.FindPath(c1, c2);
            return GetResult(result);

        }


        public async Task<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> TryResolveAsync((IVertex, IVertex) task)
        {
            BuildPathFinderWithTask(_pathFinder, task, _collisionDetector);

            var c1 = ToConnectionPoint(task.Item1, _graph, _doc);
            var c2 = ToConnectionPoint(task.Item2, _graph, _doc);

            var result = await _pathFinder.FindPathAsync(c1, c2);
            return GetResult(result);
        }

        protected abstract XYZPathFinder BuildPathFinderWithTask(XYZPathFinder pathFinder,
            (IVertex, IVertex) task, IElementCollisionDetector collisionDetector);


        protected abstract List<Element> GetElementsToExclude((IVertex, IVertex) task);

        /// <summary>
        /// Get elements to exclude from collisions by <paramref name="graph"/>.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        protected List<Element> GetExcludededByGraph(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            (IVertex, IVertex) task)
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
                var excludeIds = new List<ElementId>();

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


        private ConnectionPoint ToConnectionPoint(IVertex vertex,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, Document doc)
        {
            var pe1 = vertex.ToGraphXYZElement(graph, doc);
            var c1 = new ConnectionPoint(pe1.Item1, pe1.Item2);
            c1.GetFloorBounds(doc, 0, 0); //!!!!!
            return c1;
        }
    }
}