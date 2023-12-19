using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.GridMap;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Graphs.Validators;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using QuickGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs.Validators
{
    public class VerticedValidatorSet : IVertexValidatorSet
    {
        private readonly List<IValidator<IVertex>> _validators = new();
        private readonly UIDocument _uIDoc;
        private readonly Document _doc;
        private readonly IXYZCollisionDetector _xYZCollisionDetector;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private readonly IElementCollisionDetector _elementCollisionDetector;
        private readonly ITraceSettings _traceSettings;

        public VerticedValidatorSet(UIDocument uIDoc,
            IXYZCollisionDetector xYZCollisionDetector, 
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
           IElementCollisionDetector elementCollisionDetector, ITraceSettings traceSettings)
        {
            _uIDoc = uIDoc;
            _doc = uIDoc.Document;
            _xYZCollisionDetector = xYZCollisionDetector;
            _graph = graph;
            _elementCollisionDetector = elementCollisionDetector;
            _traceSettings = traceSettings;
        }

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<BuiltInCategory, List<PartType>> AvailableCategories { get; set; }

        /// <summary>
        /// <see cref="MEPCurve"/> to get collisions on point.
        /// </summary>
        public MEPCurve BaseMEPCurve { get; set; }


        /// <summary>
        /// Vertex bound of <see cref="Document"/>.
        /// </summary>
        public Outline ExternalOutline { get; set; }

        public double MaxLength { get; set; }

        public int MaxVerticesCount { get; set; }

        /// <summary>
        /// Specifies whether allow insulation collisions or not.
        /// </summary>
        public bool InsulationAccount { get; set; }

        public IEnumerator<IValidator<IVertex>> GetEnumerator() => _validators.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        public IVertexValidatorSet Create()
        {
            SeedVertexValidators();
            SeedGraphValidators();
            return this;
        }

        private void SeedVertexValidators()
        {
            _xYZCollisionDetector.ElementClearance = _traceSettings.B;
            var collisionValidator = new VertexCollisionValidator(_doc, _elementCollisionDetector, _xYZCollisionDetector, _graph)
            { BaseMEPCurve = BaseMEPCurve };
            var limitsValidator = new VertexLimitsValidator(_doc, _graph)
            {
                BoundOutline = ExternalOutline,
                IsInsulationAccount = InsulationAccount,
                MinDistToFloor = _traceSettings.H,
                MinDistToCeiling = _traceSettings.B
            };
            var famInstCategoryValidator = new VertexCategoryValidator(_doc, AvailableCategories);
            _validators.Add(collisionValidator);
            //_validators.Add(limitsValidator);
            _validators.Add(famInstCategoryValidator);
        }

        private void SeedGraphValidators()
        {
            var graphContainsValidator = new VertexGraphContainValidator(_doc, _graph);//always first through graph validators.
            var relationValidator = new VertexRelationValidator(_doc, _graph)
            { InElementRelation = Relation.Child, CheckVertexContainment = true };
            var vertexGraphLimitsValidator = new VertexGraphLimitsValidator(_doc, _graph)
            {
                CheckVertexContainment = true,
                MaxLength = MaxLength,
                MaxVerticesCount = MaxVerticesCount
            };

            _validators.Add(relationValidator);
            _validators.Add(graphContainsValidator);
            _validators.Add(vertexGraphLimitsValidator);
        }

    }
}
