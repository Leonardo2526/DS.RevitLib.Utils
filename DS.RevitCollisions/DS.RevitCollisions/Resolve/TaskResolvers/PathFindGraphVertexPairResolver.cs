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
using DS.ClassLib.VarUtils.GridMap;

namespace DS.RevitCollisions.Resolve.TaskResolvers
{
    public class PathFindGraphVertexPairResolver : PathFindVertexPairResolverBase
    {
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private readonly IMEPCollision _mEPCollision;
        private readonly List<MEPCurve> _baseMEPCurves = new List<MEPCurve>();

        public PathFindGraphVertexPairResolver(XYZPathFinder pathFinder, 
            Document doc, IElementCollisionDetector collisionDetector, 
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, IMEPCollision mEPCollision) : 
            base(pathFinder, doc, collisionDetector, graph)
        {
            _graph = graph;
            _mEPCollision = mEPCollision;
        }

        protected override XYZPathFinder BuildPathFinderWithTask(XYZPathFinder pathFinder,
          (IVertex, IVertex) task, IElementCollisionDetector collisionDetector)
        {
            var baseMEPCurve = _mEPCollision.Item1;
            var stateMEPCurve = _mEPCollision.Item2 as MEPCurve;

            List<Element> objectsToExclude = GetElementsToExclude(task);

            pathFinder.Graph = _graph;
            pathFinder.Build(
                baseMEPCurve,
                baseMEPCurve,
                stateMEPCurve,
                objectsToExclude, collisionDetector);

            return pathFinder;
        }

        protected override List<Element> GetElementsToExclude((IVertex, IVertex) task)
        => GetExcludededByGraph(_graph, task);
    }
}
