using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.GridMap;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
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

namespace DS.RevitCollisions
{
    public abstract class PathFindVertexPairResolverBase : 
        ITaskResolver<(IVertex, IVertex), IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>>
    {
        protected readonly IElementCollisionDetector _collisionDetector;
        protected readonly Document _doc;
        protected readonly XYZVertexPathFinder _pathFinder;
        private readonly List<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> _results = new();

        public PathFindVertexPairResolverBase(
           XYZVertexPathFinder pathFinder,
           Document doc,
           IElementCollisionDetector collisionDetector)
        {
            _pathFinder = pathFinder;
            _doc = doc;
            _collisionDetector = collisionDetector;
        }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        public IEnumerable<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> Results => _results;


        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> TryResolve((IVertex, IVertex) task)
        {
            BuildPathFinderWithTask(_pathFinder, task, _collisionDetector); 
            var result = _pathFinder.FindPath(task.Item1, task.Item2);
            return GetResult(result);
        }


        public async Task<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> TryResolveAsync((IVertex, IVertex) task)
        {
            BuildPathFinderWithTask(_pathFinder, task, _collisionDetector);
            var result = await _pathFinder.FindPathAsync(task.Item1, task.Item2);
            return GetResult(result);
        }

        protected abstract XYZVertexPathFinder BuildPathFinderWithTask(XYZVertexPathFinder pathFinder,
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
    }
}