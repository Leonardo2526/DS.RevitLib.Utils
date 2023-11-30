using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Resolvers;
using DS.ClassLib.VarUtils.Resolvers.TaskCreators;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitCollisions.Resolve.TaskCreators;
using DS.RevitCollisions.Resolve.TaskResolvers;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.PathCreators;
using QuickGraph;
using System.Collections.Generic;

namespace DS.RevitCollisions.Resolve.ResolveFactories
{

    /// <inheritdoc/>
    public class PathFindFactoryBuilder :
        FactoryBuilderBase<IMEPCollision, (IVertex, IVertex), IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>>
    {
        private string _name;
        private ITransactionFactory _transactionFactory;
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly IElementCollisionDetector _collisionDetector;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _sourceGraph;
        private readonly XYZVertexPathFinder _pathFinder;

        public PathFindFactoryBuilder(
            UIDocument uiDoc,
            IElementCollisionDetector collisionDetector,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            XYZVertexPathFinder pathFinder)
        {
            _uiDoc = uiDoc;
            _collisionDetector = collisionDetector;
            _doc = uiDoc.Document;
            _sourceGraph = graph;
            TargetGraph = graph as AdjacencyGraph<IVertex, Edge<IVertex>>;
            TargetGraph = TargetGraph.Clone();
            _pathFinder = pathFinder;
        }

        #region Properties

        /// <summary>
        /// Modified graph.
        /// </summary>
        public AdjacencyGraph<IVertex, Edge<IVertex>> TargetGraph { get; }

        public Dictionary<BuiltInCategory, List<PartType>> IterationCategories { get; set; }

        public IMEPCollision Collision { get; set; }

        public bool AutoTasks { get; set; }

        /// <summary>
        /// Messenger to show errors.
        /// </summary>
        public IWindowMessenger Messenger { get; set; }


        /// <summary>
        /// Vertex bound of <see cref="Document"/>.
        /// </summary>
        public Outline ExternalOutline { get; set; }

        /// <summary>
        /// Specifies whether allow insulation collisions or not.
        /// </summary>
        public bool InsulationAccount { get; set; }

        public ITraceSettings TraceSettings { get; set; }

        public ITransactionFactory TransactionFactory { get; set; }

        #endregion


        public PathFindFactoryBuilder WithCollision(IMEPCollision mEPCollision)
        {
            Collision = mEPCollision;
            return this;
        }


        /// <inheritdoc/>
        protected override ITaskCreator<IMEPCollision, (IVertex, IVertex)> BuildTaskCreator()
        {
            ITaskCreatorFactory<IMEPCollision, (IVertex, IVertex)> taskFactory = AutoTasks ?
                 new AutoTaskCreatorFactory(_doc, TargetGraph, Collision as MEPCollision, TraceSettings, _collisionDetector)
                 {
                     IterationCategories = IterationCategories,
                     InsulationAccount = InsulationAccount,
                     Logger = Logger,
                     TransactionFactory = TransactionFactory
                 } :
                new ManualTaskCreatorFactory(_uiDoc, TargetGraph, _collisionDetector)
                {
                    BaseMEPCurve = Collision.Item1,
                    AvailableCategories = IterationCategories,
                    ExternalOutline = ExternalOutline,
                    InsulationAccount = InsulationAccount,
                    TraceSettings = TraceSettings,
                    Messenger = Messenger,
                    Logger = Logger
                };
            return taskFactory.Create();
        }

        /// <inheritdoc/>
        protected override ITaskResolver<(IVertex, IVertex), IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> BuildTaskResover()
        {
            _pathFinder.ExternalOutline = ExternalOutline;
            _pathFinder.InsulationAccount = InsulationAccount;
            var resolver = new PathFindGraphVertexPairResolver(_pathFinder, _doc, _collisionDetector, TargetGraph, Collision)
            {
                Logger = Logger
            };
            return resolver;
        }


    }
}
