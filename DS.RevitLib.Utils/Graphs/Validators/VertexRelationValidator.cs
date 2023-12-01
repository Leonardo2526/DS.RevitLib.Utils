using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using Autodesk.Revit.UI;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// The object to validate vertex elements relations.
    /// </summary>
    public class VertexRelationValidator : IValidator<IVertex>, IValidatableObject
    {
        private readonly Document _doc;
        private readonly IBidirectionalGraph<IVertex, Edge<IVertex>> _graph;
        private List<ValidationResult> _validationResults = new();

        /// <summary>
        /// Instansiate an object to validate vertex spuds and tees for relations.
        /// <para>
        /// All other elements will be valid by default.
        /// </para>
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="graph"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public VertexRelationValidator(Document doc, IBidirectionalGraph<IVertex, Edge<IVertex>> graph)
        {
            _doc = doc;
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        /// <summary>
        /// Only input spuds and tees with specified relation will be valid.
        /// </summary>
        public Relation InElementRelation { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> ValidationResults => _validationResults;

        /// <inheritdoc/>
        public bool IsValid(IVertex value) =>
            Validate(new ValidationContext(value)).Count() == 0;

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var vertex = validationContext.ObjectInstance as IVertex;
            vertex = vertex.ToGraphVertex(_graph, _doc);
            if (vertex == null) { return _validationResults; }

            var famInst = vertex.TryGetFamilyInstance(_doc);
            if (famInst == null || (!famInst.IsSpud() && !famInst.IsTee()))
            { return _validationResults; }

            var (parents, child) = famInst.GetConnectedElements();
            var parentIds = parents is null ? new List<ElementId>() : parents.Select(x => x.Id);

            _graph.TryGetInEdges(vertex, out var inEdges);

            var inElements = vertex.GetInElements(_graph, _doc);
            var inElementsIds = inElements.Select(x => x.Id);

            switch (InElementRelation)
            {
                case Relation.Child:
                    {
                        if (child is not null && !inElementsIds.Any(id => id == child.Id))
                        { _validationResults.Add(new ValidationResult($"No elements with {InElementRelation} relation were found.")); }
                        break;
                    }
                case Relation.Parent:
                    {
                        if (parentIds.Count() > 0 && inElementsIds.Intersect(parentIds).Count() == 0)
                        { _validationResults.Add(new ValidationResult($"No elements with {InElementRelation} relation were found.")); }
                        break;
                    }
                case Relation.Default:
                    break;
                default:
                    break;
            }

            return _validationResults;
        }
    }
}
