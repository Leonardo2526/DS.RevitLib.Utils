using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Resolvers;
using DS.ClassLib.VarUtils.Resolvers.TaskCreators;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using QuickGraph;
using Serilog;
using System;
using System.Collections.Generic;

namespace DS.RevitCollisions.Resolve.TaskCreators
{
    internal class AutoTaskCreatorFactory : ITaskCreatorFactory<IMEPCollision, (IVertex, IVertex)>
    {
        private readonly Document _doc;
        private readonly AdjacencyGraph<IVertex, Edge<IVertex>> _graph;
        private readonly MEPCollision _mEPCollision;
        private readonly ITraceSettings _traceSettings;
        private readonly IElementCollisionDetector _collisionDetector;

        public AutoTaskCreatorFactory(Document doc, AdjacencyGraph<IVertex,
            Edge<IVertex>> graph, MEPCollision mEPCollision,
            ITraceSettings traceSettings, IElementCollisionDetector collisionDetector)
        {
            _doc = doc;
            _graph = graph;
            _mEPCollision = mEPCollision;
            _traceSettings = traceSettings;
            _collisionDetector = collisionDetector;
        }


        #region Properties

        /// <summary>
        /// Specifies whether allow insulation collisions or not.
        /// </summary>
        public bool InsulationAccount { get; set; }

        public ILogger Logger { get; set; }

        public ITransactionFactory TransactionFactory { get; set; }

        public Dictionary<BuiltInCategory, List<PartType>> IterationCategories { get; set; }

        #endregion


        public ITaskCreator<IMEPCollision, (IVertex, IVertex)> Create()
        {
            var pairIterator = new PairIteratorBuilder(_doc)
            {
                StartIndex = 1,
                AvailableCategories = IterationCategories,
                InElementRelation = Relation.Child
            }
            .Create(_graph);

            var segementFactory = GetFactory(_doc, _collisionDetector,
                InsulationAccount, _traceSettings, _mEPCollision);
            var pointIterator = new EdgePointIterator(segementFactory);
            var baseMEPCurve = _mEPCollision.Item1Model.MEPCurve;
            var taskCreator = new VertexPairTaskCreator(_doc, pairIterator, pointIterator, _graph, baseMEPCurve)
            {
                Logger = Logger
            };

            return taskCreator;
        }

        public SegmentFactory GetFactory(Document doc, IElementCollisionDetector collisionDetector, bool insulationAccount,
        ITraceSettings traceSettings, MEPCollision mEPCollision)
        {
            var model = mEPCollision.Item1Model;
            var mEPCurveSize = Math.Min(model.Width, model.Height);
            var insulationThickness = insulationAccount
                ? model.InsulationThickness
                : 0;

            var minDistanceToElements = mEPCurveSize / 2 + insulationThickness + traceSettings.C;
            var minDistanceToConnector = traceSettings.D + model.ElbowRadius;
            var minDistanceFromSource = (traceSettings.D + 2 * model.ElbowRadius) / Math.Tan(traceSettings.A.DegToRad());

            var segementFactory = new SegmentFactory(doc, collisionDetector)
            {
                MinDistanceToElements = minDistanceToElements,
                MinDistanceToConnector = minDistanceToConnector,
                MinDistanceFromSource = minDistanceFromSource
            };

            return segementFactory;
        }
    }
}
