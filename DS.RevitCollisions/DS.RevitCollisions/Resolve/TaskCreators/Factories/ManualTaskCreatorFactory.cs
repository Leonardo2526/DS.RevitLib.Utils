using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Resolvers;
using DS.ClassLib.VarUtils.Selectors;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.Graphs.Validators;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using DS.RevitLib.Utils.Resolve.TaskCreators;
using DS.RevitLib.Utils.Various.Selections;
using QuickGraph;
using System;
using System.Collections.Generic;

namespace DS.RevitCollisions.Resolve.TaskCreators
{
    /// <inheritdoc/>
    public class ManualTaskCreatorFactory :
        ValidatableTaskCreatorFactoryBase<IVertex>

    {
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private readonly IElementCollisionDetector _elementCollisionDetector;
        private readonly XYZCollisionDetector _xYZCollisionDetector;

        /// <inheritdoc/>
        public ManualTaskCreatorFactory(UIDocument uIDoc,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
           IElementCollisionDetector elementCollisionDetector) :
            base(uIDoc)
        {
            _graph = graph;
            _elementCollisionDetector = elementCollisionDetector;
            _xYZCollisionDetector = new XYZCollisionDetector(elementCollisionDetector);
            
        }

        public double MaxLength { get;  set; }
        public int MaxVerticesCount { get; set; }

        /// <inheritdoc/>
        public override ITaskCreator<(IVertex, IVertex)> Create()
        {           
            var vertexSelectors = new VertexSelectors(_uIDoc)
            { AllowLink = false, Logger = Logger };
            var selectors = new List<Func<IVertex>>()
            {
                vertexSelectors.SelectVertexOnElement,
                vertexSelectors.SelectVertexOnElementPoint
            };

            var validators = GetValidators();
            var selector = new ValidatableSelector<IVertex>(selectors)
            {
                Validators = validators,
                Messenger = Messenger,
                Logger = Logger
            };

            return new GraphTaskCreator(selector, _graph, _doc);
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
            var famInstCategoryValidator = new VertexCategoryValidator(_doc, AvailableCategories);

            //graph validators
            var graphContainsValidator = new VertexGraphContainValidator(_doc, _graph);//always first through graph validators.
            var relationValidator = new VertexRelationValidator(_doc, _graph)
            { InElementRelation = Relation.Child, CheckVertexContainment = true };
            var vertexGraphLimitsValidator = new VertexGraphLimitsValidator(_doc, _graph)
            {
                CheckVertexContainment = true,
                MaxLength = MaxLength, 
                MaxVerticesCount = MaxVerticesCount 
            };

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
