using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.Graphs.Validators;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using DS.RevitLib.Utils.Resolve.TaskCreators;
using QuickGraph;
using System.Collections.Generic;

namespace DS.RevitCollisions.Resolve.TaskCreators
{
    /// <inheritdoc/>
    public class ManualTaskCreatorFactory :
        ValidatableTaskCreatorFactoryBase<IMEPCollision, IVertex>

    {
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private readonly IElementCollisionDetector _elementCollisionDetector;
        private readonly XYZCollisionDetector _xYZCollisionDetector;
        private readonly IBidirectionalGraph<IVertex, Edge<IVertex>> _bdGraph;

        /// <inheritdoc/>
        public ManualTaskCreatorFactory(UIDocument uIDoc,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
           IElementCollisionDetector elementCollisionDetector) :
            base(uIDoc)
        {
            _graph = graph;
            _elementCollisionDetector = elementCollisionDetector;
            _xYZCollisionDetector = new XYZCollisionDetector(elementCollisionDetector);
            _bdGraph = graph.ToBidirectionalGraph();
        }

        public double MaxLength { get; private set; }
        public int MaxVerticesCount { get; private set; }

        /// <inheritdoc/>
        public override ITaskCreator<IMEPCollision, (IVertex, IVertex)> Create()
        {
            var validators = GetValidators();
            var selector = new VertexValidatableSelector(_uIDoc)
            {
                Validators = validators,
                Messenger = Messenger,
                Logger = Logger
            };
            return new GraphTaskCreator<IMEPCollision>(selector, _graph, _doc);
        }

        /// <inheritdoc/>
        protected override List<IValidator<IVertex>> GetValidators()
        {
            var validators = new List<IValidator<IVertex>>();

            _xYZCollisionDetector.ElementClearance = TraceSettings.B;
            var collisionValidator = new VertexCollisionValidator(_doc, _elementCollisionDetector, _xYZCollisionDetector, _graph)
            { BaseMEPCurve = BaseMEPCurve };
            var limitsValidator = new VertexLimitsValidator(_doc, _graph)
            {
                BoundOutline = ExternalOutline,
                IsInsulationAccount = InsulationAccount,
                MinDistToFloor = TraceSettings.H,
                MinDistToCeiling = TraceSettings.B
            };

            var famInstCategoryValidator = new VertexCategoryValidator(_doc, AvailableCategories, _graph);
            var relationValidator = new VertexRelationValidator(_doc, _bdGraph)
            { InElementRelation = Relation.Child };
            var graphContainsValidator = new VertexGraphContainValidator(_doc, _graph);
            var vertexGraphLimitsValidator = new VertexGraphLimitsValidator(_doc, _graph)
            { MaxLength = MaxLength, MaxVerticesCount = MaxVerticesCount };

            validators.Add(collisionValidator);
            validators.Add(limitsValidator);
            validators.Add(famInstCategoryValidator);
            validators.Add(relationValidator);
            validators.Add(graphContainsValidator);
            validators.Add(vertexGraphLimitsValidator);

            return validators;
        }
    }
}
