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
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Graphs.Validators;
using DS.RevitLib.Utils.PathCreators;
using DS.RevitLib.Utils.PathCreators.AlgorithmVertexBuilder;
using QuickGraph;
using System.Collections.Generic;

namespace DS.RevitCollisions.Resolve.ResolveFactories
{

    /// <inheritdoc/>
    public class PathFindGraphFactoryBuilder :
        FactoryBuilderBase<(IVertex, IVertex), IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>>
    {
        private ITransactionFactory _transactionFactory;
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly IElementCollisionDetector _collisionDetector;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _sourceGraph;
        private readonly XYZPathFinder _pathFinder;
        private readonly MEPCurve _baseMEPCurve;
        private readonly MEPCurve _basisMEPCurve1;
        private readonly MEPCurve _basisMEPCurve2;

        /// <summary>
        /// Instansiate a builder to create <see cref="IResolveFactory{TResult}"/> to find path
        /// between <paramref name="graph"/>'s vertices.
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <param name="collisionDetector"></param>
        /// <param name="graph"></param>
        /// <param name="pathFinder"></param>
        /// <param name="baseMEPCurve"></param>
        /// <param name="basisMEPCurve1"></param>
        /// <param name="basisMEPCurve2"></param>
        public PathFindGraphFactoryBuilder(
            UIDocument uiDoc,
            IElementCollisionDetector collisionDetector,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            XYZPathFinder pathFinder,
            MEPCurve baseMEPCurve, MEPCurve basisMEPCurve1, MEPCurve basisMEPCurve2 = null)
        {
            _uiDoc = uiDoc;
            _collisionDetector = collisionDetector;
            _doc = uiDoc.Document;
            _sourceGraph = graph;
            TargetGraph = graph as AdjacencyGraph<IVertex, Edge<IVertex>>;
            TargetGraph = TargetGraph.Clone();
            //pathFinder.Graph = TargetGraph;
            _pathFinder = pathFinder;
            _baseMEPCurve = baseMEPCurve;
            _basisMEPCurve1 = basisMEPCurve1;
            _basisMEPCurve2 = basisMEPCurve2;
        }

        #region Properties

        /// <summary>
        /// Modified graph.
        /// </summary>
        public AdjacencyGraph<IVertex, Edge<IVertex>> TargetGraph { get; }

        public Dictionary<BuiltInCategory, List<PartType>> IterationCategories { get; set; }


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



        /// <inheritdoc/>
        protected override ITaskCreator<(IVertex, IVertex)> BuildTaskCreator()
        {
            var validators =
                new VerticedValidatorSet(_uiDoc, new XYZCollisionDetector(_collisionDetector), TargetGraph, _collisionDetector, TraceSettings)
                {
                    AvailableCategories = IterationCategories,
                    BaseMEPCurve = _baseMEPCurve,
                    ExternalOutline = ExternalOutline,
                    InsulationAccount = InsulationAccount,
                    MaxLength = default,
                    MaxVerticesCount = default,
                }.Create();

            return new ManualTaskCreatorFactory(_uiDoc, TargetGraph, validators)
            {
                Messenger = Messenger,
                Logger = Logger,
                CheckAllValidators = true
            }.Create();
        }


        /// <inheritdoc/>
        protected override ITaskResolver<(IVertex, IVertex),
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> BuildTaskResover() =>
            new PathFindGraphResolver(_pathFinder, _doc, _collisionDetector, TargetGraph,
                _baseMEPCurve, _basisMEPCurve1, _basisMEPCurve2)
            {
                Logger = Logger
            };


    }
}
