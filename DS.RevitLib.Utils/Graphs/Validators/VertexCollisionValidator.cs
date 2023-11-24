using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.GridMap;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Extensions;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// The object to validate vertex collisions.
    /// </summary>
    public class VertexCollisionValidator : IValidator<IVertex>, IValidatableObject
    {
        private readonly Document _doc;
        private readonly IBidirectionalGraph<IVertex, Edge<IVertex>> _graph;
        private readonly IElementCollisionDetector _elementCollisionDetector;
        private readonly IXYZCollisionDetector _xYZCollisionDetector;
        private readonly List<ValidationResult> _validationResults = new();

        /// <summary>
        /// Instansiate object to validate vertex collisions.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="graph"></param>
        /// <param name="elementCollisionDetector"></param>
        /// <param name="xYZCollisionDetector"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public VertexCollisionValidator(Document doc, IBidirectionalGraph<IVertex, Edge<IVertex>> graph,
            IElementCollisionDetector elementCollisionDetector,
            IXYZCollisionDetector xYZCollisionDetector)
        {
            _doc = doc;
            _graph = graph;
            _elementCollisionDetector = elementCollisionDetector ??
                throw new ArgumentNullException(nameof(elementCollisionDetector));
            _xYZCollisionDetector = xYZCollisionDetector;
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

            var collisions = GetCollisions(vertex);
            if (collisions is not null && collisions.Any())
            {
                var report = GetCollisionsReport(collisions);
                _validationResults.Add(new ValidationResult(report));
            }

            return _validationResults;
        }

        /// <summary>
        /// Specifies whether point has no collisions.
        /// </summary>
        /// <returns>Returns <see langword="true"></see> if point has no collisions.
        /// <para>Otherwise returns <see langword="false"></see>.</para>
        /// </returns>
        private List<(object, Element)> GetCollisions(IVertex vertex)
        {
            var collisions = new List<(object, Element)>();

            switch (vertex)
            {
                case TaggedGVertex<Point3d> taggedPoint:
                    {
                        var xYZ = taggedPoint.Tag.ToXYZ();
                        var edge = GetEdge(vertex, xYZ);
                        if (edge is TaggedEdge<IVertex, int> taggedEdge)
                        {
                            if (_doc.GetElement(new ElementId(taggedEdge.Tag)) is MEPCurve mEPCurve)
                            { _xYZCollisionDetector.SetMEPCurve(mEPCurve); }
                        }
                        var xYZCollisions = _xYZCollisionDetector.
                            GetCollisions(xYZ);
                        xYZCollisions.ForEach(c => collisions.Add(c));
                        break;
                    }
                case TaggedGVertex<int> taggedInt:
                    {
                        var famInst = vertex.TryGetFamilyInstance(_doc);
                        if (famInst == null) { break; }
                        var fcollisions = _elementCollisionDetector.GetCollisions(famInst);
                        fcollisions.ForEach(c => collisions.Add(c));
                        break;
                    }
                default:
                    break;
            }

            return collisions;

            Edge<IVertex> GetEdge(IVertex vertex, XYZ xYZ)
            {
                var edges = new List<Edge<IVertex>>();

                if (_graph.ContainsVertex(vertex))
                {
                    _graph.TryGetInEdges(vertex, out var inEdges);
                    if (inEdges != null)
                    { edges.AddRange(inEdges); }
                }
                else
                {
                    //if vertex is not a part of a graph
                    var edge = _graph.TryGetEdge(xYZ.ToPoint3d(), _doc);
                    if (edge != null)
                    { edges.Add(edge); }
                }

                return edges.FirstOrDefault();
            }
        }

        private string GetCollisionsReport(List<(object, Element)> collisions)
        {
            var sb = new StringBuilder();
            sb.AppendLine("В точке не должно быть коллизий.");

            var elements = collisions.Select(c => c.Item2);
            var groups = elements.GroupBy(e => e.Document);

            foreach (var group in groups)
            {
                sb.AppendLine();
                sb.AppendLine($"Модель: '{group.Key.Title}': ");
                foreach (var g in group)
                {
                    sb.AppendLine("  Id: " + g.Id.IntegerValue.ToString());
                }
            }
            sb.Append("\nИтого коллизий: " + collisions.Count);

            return sb.ToString();
        }
    }
}
