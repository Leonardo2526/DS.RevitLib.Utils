using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
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
        /// Instansiate an object to validate whether <paramref name="graph"/> contains vertex.
        /// <para>
        /// All other elements will be valid by default.
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

        /// <inheritdoc/>
        public bool IsValid(IVertex value) =>
            Validate(new ValidationContext(value)).Count() == 0;

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var vertex = validationContext.ObjectInstance as IVertex;

            switch (vertex)
            {
                case TaggedGVertex<Point3d> taggedPoint:
                    {
                        var onEdge = _graph.TryGetEdge(taggedPoint.Tag, _doc);
                        if (onEdge == null) { AddMessage(); }
                        break;
                    }
                case TaggedGVertex<int> taggedPoint:
                    {
                        var onVertex = _graph.Vertices.OfType<TaggedGVertex<int>>().FirstOrDefault(v => v.Tag == taggedPoint.Tag);
                        if (onVertex.Equals(default)) { AddMessage(); }
                        break;
                    }

                default:
                    break;
            }

            return _validationResults;

            void AddMessage()
            { _validationResults.Add(new ValidationResult($"Graph doesn't contains vertex.")); }
        }
    }
}
