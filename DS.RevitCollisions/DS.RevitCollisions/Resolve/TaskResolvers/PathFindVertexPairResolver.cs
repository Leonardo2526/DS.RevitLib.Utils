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
    public class PathFindVertexPairResolver : PathFindVertexPairResolverBase
    {
        private IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private readonly IMEPCollision _mEPCollision;
        private readonly List<MEPCurve> _baseMEPCurves = new List<MEPCurve>();

        public PathFindVertexPairResolver(XYZVertexPathFinder pathFinder,
            Document doc, IElementCollisionDetector collisionDetector,
            MEPCurve baseMEPCurve, MEPCurve basisMEPCurve1, MEPCurve basisMEPCurve2 = null) :
            base(pathFinder, doc, collisionDetector)
        {
            _baseMEPCurves = new List<MEPCurve>()
                {baseMEPCurve, basisMEPCurve1};
            if (basisMEPCurve2 != null) { _baseMEPCurves.Add(basisMEPCurve2); }
        }

        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> Graph 
        { get => _graph; set => _graph = value; }

        protected override XYZVertexPathFinder BuildPathFinderWithTask(XYZVertexPathFinder pathFinder,
          (IVertex, IVertex) task, IElementCollisionDetector collisionDetector)
        {
            
            List<Element> objectsToExclude = Graph is null ? 
                GetElementsToExclude(task) : 
                GetExcludededByGraph(Graph, task);

            var basisMEPCurve2 = _baseMEPCurves.Count > 2 ? _baseMEPCurves[2] : null;

            pathFinder.Build(_graph,
                _baseMEPCurves[0],
                _baseMEPCurves[1],
                basisMEPCurve2,
                objectsToExclude, collisionDetector);

            return pathFinder;
        }

        /// <summary>
        /// Get elements to exclude from collisions.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        protected override List<Element> GetElementsToExclude((IVertex, IVertex) task)
        {
            var objectsToExclude = new List<Element>();

            var excluded1 = GetExcluded(task.Item1);
            var excluded2 = GetExcluded(task.Item2);
            objectsToExclude.AddRange(excluded1);
            objectsToExclude.AddRange(excluded2);

            return objectsToExclude;


            IEnumerable<Element> GetExcluded(IVertex vertex)
            {
                var objectsToExclude = new List<Element>();

                switch (vertex)
                {
                    case TaggedGVertex<int> taggedInt:
                        {
                            var famInst = taggedInt.TryGetFamilyInstance(_doc);
                            if (famInst != null)
                            { objectsToExclude.Add(famInst); }
                            break;
                        }
                    case TaggedGVertex<Point3d> taggedPoint:
                        {
                            break;
                        }
                    case TaggedGVertex<(int, Point3d)> taggedIntPoint:
                        {
                            var element = _doc.GetElement(new ElementId(taggedIntPoint.Tag.Item1));
                            if (element != null)
                            { objectsToExclude.Add(element); }
                            break;
                        }

                    default:
                        break;
                }

                return objectsToExclude;
            }
        }
    }
}
