using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.GridMap;
using DS.GraphUtils.Entities;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// The object to validate whether graph contains vertex. 
    /// </summary>
    public class VertexGraphContainValidator : IValidator<IVertex>, IValidatableObject
    {
        private readonly Document _doc;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private List<ValidationResult> _validationResults = new();

        /// <summary>
        /// Instansiate an object to validate whether <paramref name="graph"/> can contains vertex.
        /// <para>
        /// If graph doesnt'n contains vertex it tries to insert it into graph.
        /// if it was failed, add validation error.
        /// </para>
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="graph"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public VertexGraphContainValidator(Document doc, IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            _doc = doc;
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> ValidationResults => _validationResults;

        //public bool CanModifyGraph { get; set; } = false;

        /// <inheritdoc/>
        public bool IsValid(IVertex value) =>
            Validate(new ValidationContext(value)).Count() == 0;

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            _validationResults.Clear();
            var vertex = validationContext.ObjectInstance as IVertex;

            if (!_graph.TryFindItemByTag(vertex, _doc, out var foundVertex, out var foundEdge))
            { _validationResults.Add(new ValidationResult($"Graph doesn't contains vertex.")); }
            return _validationResults;

        }
    }
}
