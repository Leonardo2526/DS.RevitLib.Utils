﻿using Autodesk.Revit.DB;
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
    public class GraphValidatorSet : IVertexValidatorSet
    {
        private readonly List<IValidator<IVertex>> _validators = new();
        private readonly UIDocument _uIDoc;
        private readonly Document _doc;
        private readonly IXYZCollisionDetector _xYZCollisionDetector;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private readonly IElementCollisionDetector _elementCollisionDetector;
        private readonly ITraceSettings _traceSettings;

        public GraphValidatorSet(UIDocument uIDoc,
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
        /// Only input spuds and tees with specified relation will be iterated.
        /// </summary>
        public Relation InElementRelation { get; set; }

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
            _xYZCollisionDetector.ElementClearance = _traceSettings.B;
            var collisionValidator = new VertexCollisionValidator(_doc, _elementCollisionDetector, _xYZCollisionDetector, _graph)
            { BaseMEPCurve = BaseMEPCurve };
            _validators.Add(collisionValidator);

            if (AvailableCategories != null && AvailableCategories.Count > 0)
            {
                var catValidator = new VertexCategoryValidator(_doc, AvailableCategories);
                _validators.Add(catValidator);
            }


            if (InElementRelation != Relation.Default)
            {
                var relationValidator = new VertexRelationValidator(_doc, _graph)
                {
                    InElementRelation = InElementRelation
                };
                _validators.Add(relationValidator);
            }

            return this;
        }
    }
}