using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Resolvers;
using DS.ClassLib.VarUtils.Resolvers.TaskCreators;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using QuickGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.MEP;

namespace DS.RevitCollisions.Resolve.Impl.PathFindFactoryBuilder
{
    internal class AutoTaskCreatorFactory : ITaskCreatorFactory<IMEPCollision, (IVertex, IVertex)>
    {
        private readonly Document _doc;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;

        public AutoTaskCreatorFactory(Document doc, IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            _doc = doc;
            _graph = graph;
        }

        public Dictionary<BuiltInCategory, List<PartType>> IterationCategories { get; set; }

        public ITaskCreator<IMEPCollision, (IVertex, IVertex)> Create()
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
