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
using System.Linq;

namespace DS.RevitCollisions.Resolve.TaskCreators
{
    /// <inheritdoc/>
    public class ManualTaskCreatorFactory :
        ValidatableTaskCreatorFactoryBase<IVertex>

    {
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private readonly IVertexValidatorSet _validators;

        /// <inheritdoc/>
        public ManualTaskCreatorFactory(UIDocument uIDoc,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
           IVertexValidatorSet validators) :
            base(uIDoc)
        {
            _graph = graph;
            _validators = validators;
            
        }

        /// <summary>
        /// Validate object by all validators or only to first with not valid result.
        /// </summary>
        public bool CheckAllValidators { get; set; }

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
                Logger = Logger,
                CheckAllValidators = CheckAllValidators
            };

            return new GraphTaskCreator(selector, _graph, _doc);
        }

        /// <inheritdoc/>
        protected override List<IValidator<IVertex>> GetValidators()
            => _validators.ToList();
    }
}
