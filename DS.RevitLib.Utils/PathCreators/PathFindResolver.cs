using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.GridMap;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.PathFinder;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using QuickGraph;
using Rhino.Geometry;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.PathCreators
{
    /// <summary>
    /// Resolver to resolve <see cref="ConnectionPoint"/> tasks.
    /// </summary>
    public class PathFindResolver :
        ITaskResolver<((Element, XYZ), (Element, XYZ)), IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>>
    {
        /// <summary>
        /// Current active <see cref="Document"/>.
        /// </summary>
        protected readonly Document _doc;

        protected readonly XYZPathFinder _pathFinder;
        private readonly IElementCollisionDetector _collisionDetector;
        private readonly MEPCurve _baseMEPCurve;
        private readonly MEPCurve _basisMEPCurve1;
        private readonly MEPCurve _basisMEPCurve2;
        private readonly List<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> _results = new();

        /// <summary>
        /// Instansiate a resolver to resolve <see cref="ConnectionPoint"/> tasks.
        /// </summary>
        /// <param name="pathFinder"></param>
        /// <param name="doc"></param>
        /// <param name="collisionDetector"></param>
        /// <param name="baseMEPCurve"></param>
        /// <param name="basisMEPCurve1"></param>
        /// <param name="basisMEPCurve2"></param>
        public PathFindResolver(
           XYZPathFinder pathFinder,
           Document doc,
           IElementCollisionDetector collisionDetector,
            MEPCurve baseMEPCurve, MEPCurve basisMEPCurve1, MEPCurve basisMEPCurve2 = null)
        {
            _pathFinder = pathFinder;
            _doc = doc;
            _collisionDetector = collisionDetector;
            _baseMEPCurve = baseMEPCurve;
            _basisMEPCurve1 = basisMEPCurve1;
            _basisMEPCurve2 = basisMEPCurve2;
        }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> Results => _results;

        /// <inheritdoc/>
        public CancellationTokenSource CancellationToken { get; set; }

        /// <inheritdoc/>
        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> TryResolve(((Element, XYZ), (Element, XYZ)) task)
        {
            var c1 = new ConnectionPoint(task.Item1.Item1, task.Item1.Item2);
            var c2 = new ConnectionPoint(task.Item2.Item1, task.Item2.Item2);
            _pathFinder.ExternalToken = CancellationToken;
            BuildPathFinderWithTask(_pathFinder, (c1, c2), _collisionDetector);

            var result = _pathFinder.FindPath(c1, c2);
            return ConvertToGraph(result);
        }

        /// <inheritdoc/>
        public async Task<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> TryResolveAsync(((Element, XYZ), (Element, XYZ)) task)
        {
            var c1 = new ConnectionPoint(task.Item1.Item1, task.Item1.Item2);
            var c2 = new ConnectionPoint(task.Item2.Item1, task.Item2.Item2);
            _pathFinder.ExternalToken = CancellationToken;
            BuildPathFinderWithTask(_pathFinder, (c1, c2), _collisionDetector);

            var result = _pathFinder.FindPath(c1, c2);
            //var result = await _pathFinder.FindPathAsync(c1, c2);
            return ConvertToGraph(result);
        }

        private XYZPathFinder BuildPathFinderWithTask(XYZPathFinder pathFinder,
      (ConnectionPoint, ConnectionPoint) task, IElementCollisionDetector collisionDetector)
        {
            List<Element> objectsToExclude = GetElementsToExclude(task);

            pathFinder.Build(
                _baseMEPCurve,
                _basisMEPCurve1,
                _basisMEPCurve2,
                objectsToExclude, collisionDetector);
            return pathFinder;
        }

        /// <summary>
        /// Get elements to exclude from collisions.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        protected virtual List<Element> GetElementsToExclude((ConnectionPoint, ConnectionPoint) task)
        {
            List<Element> objectsToExclude = new List<Element>()
            {
                task.Item1.Element,
                task.Item2.Element
            };

            return objectsToExclude;
        }

        private IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> ConvertToGraph(List<XYZ> pathPoints)
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
