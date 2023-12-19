using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Resolvers;
using DS.ClassLib.VarUtils.Resolvers.TaskCreators;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.Graphs.Validators;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using DS.RevitLib.Utils.Resolve.TaskCreators;
using QuickGraph;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitCollisions.Resolve.TaskCreators
{
    internal class AutoTaskCreatorFactory : ValidatableTaskCreatorFactoryBase<IVertex>
    {
        private readonly AdjacencyGraph<IVertex, Edge<IVertex>> _graph;
        private readonly MEPCollision _mEPCollision;
        private readonly ITraceSettings _traceSettings;
        private readonly IElementCollisionDetector _collisionDetector;
        private readonly IVertexValidatorSet _validators;

        public AutoTaskCreatorFactory(UIDocument uIDoc, 
            AdjacencyGraph<IVertex, Edge<IVertex>> graph, 
            MEPCollision mEPCollision, 
            ITraceSettings traceSettings,
            IElementCollisionDetector collisionDetector, 
            IVertexValidatorSet validators) : base(uIDoc)
        {
            _graph = graph;
            _mEPCollision = mEPCollision;
            _traceSettings = traceSettings;
            _collisionDetector = collisionDetector;
            _validators = validators;
        }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<BuiltInCategory, List<PartType>> AvailableCategories { get; set; }

        /// <summary>
        /// Specifies whether allow insulation collisions or not.
        /// </summary>
        public bool InsulationAccount { get; set; }

        public override ITaskCreator<(IVertex, IVertex)> Create()
        {
            var pairIterator = new PairIteratorBuilder(_doc, _validators)
            {
                StartIndex = 1,
                AvailableCategories = AvailableCategories,
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
            var minDistanceToConnector = traceSettings.D + _traceSettings.R;
            var minDistanceFromSource = (traceSettings.D + 2 * _traceSettings.R) / Math.Tan(traceSettings.A.DegToRad());

            var segementFactory = new SegmentFactory(doc, collisionDetector)
            {
                MinDistanceToElements = minDistanceToElements,
                MinDistanceToConnector = minDistanceToConnector,
                MinDistanceFromSource = minDistanceFromSource, 
                IsInsulationAccount = insulationAccount,
            };

            return segementFactory;
        }

        protected override List<IValidator<IVertex>> GetValidators() => _validators.ToList();
    }
}
