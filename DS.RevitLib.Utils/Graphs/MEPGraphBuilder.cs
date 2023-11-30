using Autodesk.Revit.DB;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using QuickGraph;
using System;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// An object that represents builder for graph.
    /// </summary>
    public class MEPGraphBuilder : IMEPGraphBuilder
    {
        private readonly Document _doc;
        private MEPSystemGraphFactory _graphFactory;

        /// <summary>
        /// Instansiate an object to build for graph.
        /// </summary>
        public MEPGraphBuilder(Document doc)
        {
            _doc = doc;
        }

        #region Properties

        /// <summary>
        /// Veritces <see cref="Type"/>'s to stop for graph building.
        /// </summary>
        public IEnumerable<Type> StopTypes { get; set; }

        /// <summary>
        /// Veritces <see cref="Autodesk.Revit.DB.BuiltInCategory"/>'s to stop for graph building.
        /// </summary>
        public Dictionary<BuiltInCategory, List<PartType>> StopCategories { get; set; }

        /// <summary>
        /// Spuds and tees relaion to stop graph building.
        /// </summary>
        public Relation StopElementRelation { get; set; }

        /// <summary>
        /// Vertex bound of <see cref="Document"/>.
        /// </summary>
        public Outline BoundOutline { get; set; }

        /// <summary>
        /// <see cref="Type"/>'s to exclude for verices build.
        /// </summary>
        public IEnumerable<Type> ExcludedTypes { get; set; }

        /// <summary>
        /// <see cref="Autodesk.Revit.DB.BuiltInCategory"/>'s to exclude for verices build.
        /// </summary>
        public Dictionary<BuiltInCategory, List<PartType>> ExculdedCategories { get; set; }

        /// <summary>
        /// Maximum length from <see cref="IVertex"/> to graph root vertex.
        /// </summary>
        public double MaxLength { get; set; }

        /// <summary>
        /// Maximum vertices count from <see cref="IVertex"/> to graph root vertex.
        /// </summary>
        public int MaxVerticesCount { get; set; }


        #endregion

        /// <inheritdoc/>
        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> Create(MEPCurve mEPCurve, XYZ pointOnMEPCurve)
        {
            CreateGraphFactory();
            if (StopElementRelation != Relation.Default)
            { this.WithRelationValidator(); }

            return _graphFactory.Create(mEPCurve, pointOnMEPCurve);
        }

        /// <inheritdoc/>
        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> Create(Element element)
        {
            CreateGraphFactory();
            if (StopElementRelation != Relation.Default)
            { this.WithRelationValidator(); }

            return _graphFactory.Create(element);
        }


        private MEPGraphBuilder CreateGraphFactory()
        {
            var vertexBuilder = CreateVertexBuider(_doc);
            var edgeBuilder = new GEdgeBuilder();

            _graphFactory = new MEPSystemGraphFactory(_doc, vertexBuilder, edgeBuilder)
            {
                //TransactionFactory = _trfIn,
                //UIDoc = _uiDoc,
                StopTypes = StopTypes,
                StopCategories = StopCategories
            };

            return this;
        }

        private IVertexBuilder<IVertex, TaggedGVertex<int>> CreateVertexBuider(Document doc)
        {
            var validator = new VertexAllLimitsValidator(doc)
            {
                MaxLength = MaxLength,
                MaxVerticesCount = MaxVerticesCount,
                BoundOutline = BoundOutline,
                ExcludedTypes = ExcludedTypes,
                ExculdedCategories = ExculdedCategories
            };

            var vertexBuilder = new GVertexBuilder(doc)
            {
                Validatator = validator
            };
            return vertexBuilder;
        }


        private MEPSystemGraphFactoryBase<AdjacencyGraph<IVertex, Edge<IVertex>>> WithRelationValidator()
        {
            var relationValidator = new VertexRelationValidator(_doc, _graphFactory.Graph.ToBidirectionalGraph())
            {
                InElementRelation = Relation.Parent
            };
            _graphFactory.StopRelationValidator = relationValidator;

            return _graphFactory;
        }

    }
}
