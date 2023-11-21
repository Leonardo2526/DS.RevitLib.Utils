using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using QuickGraph;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitCollisions.Impl
{
    /// <inheritdoc/>
    partial class PathFindFactoryBuilder : MEPCollisionFactoryBuilderBase<(IVertex, IVertex), PointsList>
    {
        /// <inheritdoc/>
        protected override ITaskCreator<IMEPCollision, (IVertex, IVertex)> BuildTaskCreator()
        {
            var pairIterator = new PairIteratorBuilder(_doc)
            {
                StartIndex = 1,
                AvailableCategories = IterationCategories,
                InElementRelation = Relation.Child
            }
            .Create(_graph);

            var agraph = (AdjacencyGraph<IVertex, Edge<IVertex>>)_graph;
            var taskCreator = new VertexPairTaskCreator(pairIterator)
            {
                InitialTasks = GetInitialTasks(pairIterator, agraph)
            };

            return taskCreator;
        }



        private IEnumerable<(IVertex, IVertex)> GetInitialTasks(IEnumerator<(IVertex, IVertex)> pairIterator,
            AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
            List<(IVertex, IVertex)> list = new();

            while (list.Count != 4 && pairIterator.MoveNext())
            { list.Add(pairIterator.Current); }

            var firstEdge = graph.Edges.First() as TaggedEdge<IVertex, int>;
            //return;
            var baseElement = _doc.GetElement(new ElementId(firstEdge.Tag)) as MEPCurve;
            double sizeFactor = baseElement.GetMaxSize();
            return list.SortByTaggedLength(graph, _doc, 25, sizeFactor).ToList();
        }
    }
}
