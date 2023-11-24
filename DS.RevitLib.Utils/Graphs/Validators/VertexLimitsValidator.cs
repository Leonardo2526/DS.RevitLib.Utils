using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Extensions;
using QuickGraph;
using QuickGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Validator for <see cref="IVertex"/> verices.
    /// </summary>
    public class VertexLimitsValidator : IVertexLimitsValidator, IValidator<IVertex>, IValidatableObject
    {
        private readonly Document _doc;
        private IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private IVertex _parentVertex;
        private readonly List<ValidationResult> _validationResults = new();

        /// <summary>
        /// Create validator to check <see cref="IVertex"/>  vertices.
        /// </summary>
        /// <param name="doc"></param>
        public VertexLimitsValidator(Document doc)
        {
            _doc = doc;
        }


        #region Properties

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

        public bool IsInsulationAccount { get; set; }
        public double MinDistToFloor { get; set; }
        public double MinDistToCeiling { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> ValidationResults => _validationResults;

        #endregion


        /// <inheritdoc/>
        public void Instansiate(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            _graph = graph;
        }

        /// <inheritdoc/>
        public IVertexLimitsValidator SetParent(IVertex parentVertex)
        {
            _parentVertex = parentVertex;
            return this;
        }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            var vertex = validationContext.ObjectInstance as IVertex;

            if (!IsWithinOutlineLimits(BoundOutline, vertex))
            { results.Add(new ValidationResult("Vertex is outside bound limits.")); }

            if (!IsWithinMaxLength(_graph, _parentVertex, vertex))
            { results.Add(new ValidationResult("Vertex is outside MaxLength.")); }

            if (!IsWithinFloorsBounds(vertex, _doc, MinDistToFloor, MinDistToCeiling, IsInsulationAccount))
            { results.Add(new ValidationResult("Excluded types contains vertex.")); }

            if (vertex.ContainsCategories(ExculdedCategories, _doc))
            { results.Add(new ValidationResult("Excluded categories contains vertex.")); }

            if (vertex.ContainsTypes(ExcludedTypes, _doc))
            { results.Add(new ValidationResult("Excluded types contains vertex.")); }

            return results;
        }


        #region PrivateMethods

        private bool IsWithinOutlineLimits(Outline boundOutline, IVertex vertex)
        {
            if (boundOutline == null) { return true; }
            var point = vertex.GetLocation(_doc);
            return point.More(boundOutline.MinimumPoint) && point.Less(boundOutline.MaximumPoint);

        }

        private bool IsWithinMaxLength(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, IVertex parentVertex, IVertex vertex)
        {
            if (MaxLength == 0 && MaxVerticesCount == 0) { return true; }

            var roots = graph.Roots();
            if (roots.Count() == 0) { return true; }

            if (graph is not AdjacencyGraph<IVertex, Edge<IVertex>> aGraph) { return true; }
            var gc = aGraph.Clone();

            gc.AddVertex(vertex);
            var edge = new TaggedEdge<IVertex, int>(parentVertex, vertex, 0);
            gc.AddEdge(edge);

            var (length, verticesCount) = gc.GetLengthToRoot(vertex, _doc);

            var isLengthValid = MaxLength == 0 || length <= MaxLength;
            var isCountValid = MaxVerticesCount == 0 || verticesCount <= MaxVerticesCount;

            return isLengthValid && isCountValid;
        }


        private bool IsWithinFloorsBounds(IVertex vertex, Document doc,
        double minDistToFloor, double minDistToCeiling,
        bool isInsulationAccount, int distnaceToFindFloor = 30)
        {
            if (minDistToFloor == 0 && minDistToCeiling == 0) { return true; }

            var element = vertex.TryGetElementFromGraphAndDoc(_graph.ToBidirectionalGraph(), _doc);
            var point = vertex.GetLocation(doc);

            var (pointFloorBound, pointCeilingBound) =
                GetFloorBounds((element, point), doc, minDistToFloor, minDistToCeiling, isInsulationAccount, distnaceToFindFloor);

            return pointFloorBound != null && pointCeilingBound != null;

            static (XYZ pointFloorBound, XYZ pointCeilingBound) GetFloorBounds(
                 (Element, XYZ) pointElement,
            Document doc,
            double minDistToFloor, double minDistToCeiling,
            bool isInsulationAccount,
            int distnaceToFindFloor)
            {
                return pointElement.GetFloorBounds(doc, minDistToFloor, minDistToCeiling,
                    isInsulationAccount, distnaceToFindFloor);
            }
        }

        /// <inheritdoc/>
        public bool IsValid(IVertex value) =>
            Validate(new ValidationContext(value)).Count() == 0;


        #endregion

    }
}
