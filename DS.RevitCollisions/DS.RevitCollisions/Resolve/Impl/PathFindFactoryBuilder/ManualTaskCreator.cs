using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.Various;
using QuickGraph;
using Serilog;

namespace DS.RevitCollisions.Resolve.Impl.PathFindFactoryBuilder
{
    /// <summary>
    /// An object for manual create tasks to resolve collision.
    /// </summary>
    public class ManualTaskCreator : ITaskCreator<IMEPCollision, (IVertex, IVertex)>
    {
        private readonly IValidatableSelector<IVertex> _selector;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private readonly Document _doc;

        /// <summary>
        /// Instantiate an object for manual create tasks to resolve collision.
        /// </summary>
        /// <param name="collision"></param>
        /// <param name="selector"></param>
        public ManualTaskCreator(IValidatableSelector<IVertex> selector,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, Document doc)
        {
            _selector = selector;
            _graph = graph;
            _doc = doc;
        }

        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        public (IVertex, IVertex) CreateTask(IMEPCollision item)
        {
            var v1 = _selector.Select();
            if (v1 == null) { return (null, null); }

            var v2 = _selector.Select();
            if (v2 == null) { return (null, null); }

            var graphVertex1 = v1.ToGraphVertex(_graph, _doc);
            var graphVertex2 = v2.ToGraphVertex(_graph, _doc);

            return (graphVertex1, graphVertex2);
        }
    }
}
