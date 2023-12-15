using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.GridMap;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.PathFinder;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Graphs;
using QuickGraph;
using QuickGraph.Algorithms;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace DS.RevitLib.Utils.PathCreators
{
    /// <summary>
    /// Resolver to resolve <see cref="IVertex"/> tasks.
    /// </summary>
    public class PathFindGraphResolver :
       PathFindResolver, ITaskResolver<(IVertex, IVertex), IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>>
    {
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private List<Element> _excluded;

        /// <summary>
        /// Instansiate a resolver to resolve <see cref="IVertex"/> tasks.
        /// </summary>
        /// <param name="pathFinder"></param>
        /// <param name="doc"></param>
        /// <param name="collisionDetector"></param>
        /// <param name="graph"></param>
        /// <param name="baseMEPCurve"></param>
        /// <param name="basisMEPCurve1"></param>
        /// <param name="basisMEPCurve2"></param>
        public PathFindGraphResolver(XYZPathFinder pathFinder, Document doc,
            IElementCollisionDetector collisionDetector, IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            MEPCurve baseMEPCurve, MEPCurve basisMEPCurve1, MEPCurve basisMEPCurve2 = null) :
            base(pathFinder, doc, collisionDetector, baseMEPCurve, basisMEPCurve1, basisMEPCurve2)
        {
            _graph = graph;
        }

        /// <inheritdoc/>
        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> TryResolve((IVertex, IVertex) task)
        {
            _pathFinder.Graph = _graph;

            var c1 = ToConnectionPoint(task.Item1, _graph, _doc);
            var c2 = ToConnectionPoint(task.Item2, _graph, _doc);
            _excluded = GetElementsToExclude(task);

            var taskItem1 = (c1.Element, c1.Point);
            var taskItem2 = (c2.Element, c2.Point);

            return TryResolve((taskItem1, taskItem2));
        }

        /// <inheritdoc/>
        public async Task<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> TryResolveAsync((IVertex, IVertex) task)
        {
            _pathFinder.Graph = _graph;

            var c1 = ToConnectionPoint(task.Item1, _graph, _doc);
            var c2 = ToConnectionPoint(task.Item2, _graph, _doc);
            _excluded = GetElementsToExclude(task);

            var taskItem1 = (c1.Element, c1.Point);
            var taskItem2 = (c2.Element, c2.Point);

            return await TryResolveAsync((taskItem1, taskItem2));
        }

        /// <inheritdoc/>
        protected override List<Element> GetElementsToExclude((ConnectionPoint, ConnectionPoint) task)
        => _excluded;

        private List<Element> GetElementsToExclude((IVertex, IVertex) task)
        {
            var objectsToExclude = new List<Element>();

            var idsToExclude = new List<ElementId>();
            var root = _graph.Roots().First();
            var idsToExclude1 = _graph.GetElementIds(root, task.Item1, _doc);
            var idsToExclude2 = _graph.GetElementIds(root, task.Item2, _doc);
            idsToExclude.AddRange(idsToExclude1);
            idsToExclude.AddRange(idsToExclude2);

            //add outElements
            _graph.TryGetOutEdges(task.Item1, out var e1);
            idsToExclude.AddRange(GetOutIds(e1));
            _graph.TryGetOutEdges(task.Item2, out var e2);
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

        private ConnectionPoint ToConnectionPoint(IVertex vertex,
         IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, Document doc)
        {
            var pe1 = vertex.ToGraphXYZElement(graph, doc);
            var c1 = new ConnectionPoint(pe1.Item1, pe1.Item2);
            return c1;
        }
    }
}
