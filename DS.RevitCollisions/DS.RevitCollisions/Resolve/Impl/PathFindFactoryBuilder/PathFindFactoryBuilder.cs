using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.PathFinder;
using DS.RevitCollisions.Models;
using DS.RevitCollisions.Resolve.Impl.PathFindFactoryBuilder;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.PathCreators;
using QuickGraph;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Threading;

namespace DS.RevitCollisions.Impl
{

    /// <inheritdoc/>
    public partial class PathFindFactoryBuilder : 
        FactoryBuilderBase<IMEPCollision, (IVertex, IVertex), IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> 
    {
        private string _name;
        private ITransactionFactory _transactionFactory;
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly IElementCollisionDetector _collisionDetector;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
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
            _graph = graph;
            _pathFinder = pathFinder;
        }

        #region Properties

        public Dictionary<BuiltInCategory, List<PartType>> IterationCategories { get; set; }

        public IMEPCollision Collision { get; set; }

        public bool AutoTasks { get; set; }

        #endregion


        public PathFindFactoryBuilder WithCollision(IMEPCollision mEPCollision)
        {
            Collision = mEPCollision;
            return this;
        }


        /// <inheritdoc/>
        protected override ITaskCreator<IMEPCollision, (IVertex, IVertex)> BuildTaskCreator()
        {
            var taskFactory = AutoTasks ? new AutoTaskCreatorFactory(_doc, _graph) : null;
            return taskFactory.Create();
        }

        /// <inheritdoc/>
        protected override ITaskResolver<(IVertex, IVertex), IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> BuildTaskResover()
        {
            var resolver = new PathFindVertexPairResolver(_pathFinder, _graph, _doc, Collision, _collisionDetector);
            return resolver;
        }


    }
}
