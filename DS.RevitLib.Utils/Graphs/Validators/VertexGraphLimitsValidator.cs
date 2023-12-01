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
    /// Validator to check limit of <see cref="IVertex"/> verices inside graph.
    /// </summary>
    public class VertexGraphLimitsValidator : IValidator<IVertex>, IValidatableObject
    {
        private readonly Document _doc;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private readonly List<ValidationResult> _validationResults = new();
        private IVertex _parentVertex;

        /// <summary>
        /// Create validator to check limit of <see cref="IVertex"/> verices inside <paramref name="graph"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="graph"></param>
        public VertexGraphLimitsValidator(Document doc, IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            _doc = doc;
            _graph = graph;
        }


        #region Properties

        /// <summary>
        /// Maximum length from <see cref="IVertex"/> to graph root vertex.
        /// </summary>
        public double MaxLength { get; set; }

        /// <summary>
        /// Maximum vertices count from <see cref="IVertex"/> to graph root vertex.
        /// </summary>
        public int MaxVerticesCount { get; set; }

        /// <summary>
        /// Parent <see cref="IVertex"/> to check limits.
        /// </summary>
        public IVertex ParentVertex { get => _parentVertex; set => _parentVertex = value; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> ValidationResults => _validationResults;

        #endregion

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            var vertex = validationContext.ObjectInstance as IVertex;
            vertex = vertex.ToGraphVertex(_graph, _doc);
            if(vertex == null) { return results; }

            if (!IsWithinMaxLength(_graph, _parentVertex, vertex))
            { results.Add(new ValidationResult("Vertex is outside MaxLength.")); }

            return results;
        }


        #region PrivateMethods

        private bool IsWithinMaxLength(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph,
            IVertex parentVertex, IVertex vertex)
        {
            if (MaxLength == 0 && MaxVerticesCount == 0) { return true; }

            var roots = graph.Roots();
            if (roots.Count() == 0) { return true; }

            if (graph is not AdjacencyGraph<IVertex, Edge<IVertex>> aGraph) { return true; }
            var gc = aGraph.Clone();

            gc.AddVertex(vertex);
            parentVertex ??= _graph.Roots().First();
            var edge = new TaggedEdge<IVertex, int>(parentVertex, vertex, 0);
            gc.AddEdge(edge);

            var (length, verticesCount) = gc.GetLengthToRoot(vertex, _doc);

            var isLengthValid = MaxLength == 0 || length <= MaxLength;
            var isCountValid = MaxVerticesCount == 0 || verticesCount <= MaxVerticesCount;

            return isLengthValid && isCountValid;
        }

        /// <inheritdoc/>
        public bool IsValid(IVertex value) =>
            Validate(new ValidationContext(value)).Count() == 0;

        #endregion

    }
}
