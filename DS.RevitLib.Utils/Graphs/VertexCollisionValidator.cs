using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Graphs;
using DS.ClassLib.VarUtils.Graphs.Vertices;
using DS.ClassLib.VarUtils.GridMap;
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
        public bool IsValid(IVertex value) =>
            Validate(new ValidationContext(value)).Count() == 0;

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            var vertex = validationContext.ObjectInstance as IVertex;

            var collisions = GetCollisions(vertex);
            if (collisions is not null && collisions.Any())
            {
                var report = GetCollisionsReport(collisions);
                results.Add(new ValidationResult(report));
            }

            return results;
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
                        _graph.TryGetInEdges(vertex, out var inEdeges);
                        if (inEdeges.FirstOrDefault() is TaggedEdge<IVertex, int> taggedEdge)
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
