using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitCollisions.Resolve.TaskCreators;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Graphs.Validators;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using DS.RevitLib.Utils.PathCreators;
using QuickGraph;
using System.Collections.Generic;

namespace DS.RevitCollisions.Resolve.ResolveFactories
{

    /// <inheritdoc/>
    public class PathFindCollisionFactoryBuilder :
        FactoryBuilderBase<(IVertex, IVertex), IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>>
    {
        private ITransactionFactory _transactionFactory;
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly IElementCollisionDetector _collisionDetector;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _sourceGraph;
        private readonly XYZPathFinder _pathFinder;
        private readonly MEPCollision _mEPCollision;

        /// <summary>
        /// Instansiate a builder to create <see cref="IResolveFactory{TResult}"/> 
        /// to resolve <see cref="MEPCollision"/>
        /// by finding path between <paramref name="graph"/>'s vertices.
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <param name="collisionDetector"></param>
        /// <param name="graph"></param>
        /// <param name="pathFinder"></param>
        /// <param name="mEPCollision"></param>
        public PathFindCollisionFactoryBuilder(
            UIDocument uiDoc,
            IElementCollisionDetector collisionDetector,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            XYZPathFinder pathFinder, MEPCollision mEPCollision)
        {
            _uiDoc = uiDoc;
            _collisionDetector = collisionDetector;
            _doc = uiDoc.Document;
            _sourceGraph = graph;
            TargetGraph = graph as AdjacencyGraph<IVertex, Edge<IVertex>>;
            TargetGraph = TargetGraph.Clone();
            //pathFinder.Graph = TargetGraph;
            _pathFinder = pathFinder;
            _mEPCollision = mEPCollision;
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
               new GraphValidatorSet(_uiDoc, new XYZCollisionDetector(_collisionDetector), TargetGraph, _collisionDetector, TraceSettings)
               {
                   AvailableCategories = IterationCategories,
                   BaseMEPCurve = _mEPCollision.Item1Model.MEPCurve,
                   ExternalOutline = ExternalOutline,
                   InsulationAccount = InsulationAccount,
                   MaxLength = default,
                   MaxVerticesCount = default, 
                   InElementRelation = Relation.Child
               }.Create();

            return new AutoTaskCreatorFactory(_uiDoc, TargetGraph, _mEPCollision, TraceSettings, _collisionDetector, validators)
            {
                InsulationAccount = InsulationAccount,
                Logger = Logger
            }.Create();
        }

        /// <inheritdoc/>
        protected override ITaskResolver<(IVertex, IVertex),
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> BuildTaskResover() =>
            new PathFindGraphResolver(_pathFinder, _doc, _collisionDetector, TargetGraph,
                _mEPCollision.Item1Model.MEPCurve, _mEPCollision.Item1Model.MEPCurve, _mEPCollision.Item2 as MEPCurve)
            {
                Logger = Logger
            };

    }
}
