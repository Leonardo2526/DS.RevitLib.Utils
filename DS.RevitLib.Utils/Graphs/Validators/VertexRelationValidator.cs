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
using Rhino.Geometry;
using QuickGraph.Algorithms;
using Autodesk.Revit.DB.Visual;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// The object to validate vertex elements relations.
    /// </summary>
    public class VertexRelationValidator : IValidator<IVertex>, IValidatableObject
    {
        private readonly Document _doc;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private List<ValidationResult> _validationResults = new();
        private IBidirectionalGraph<IVertex, Edge<IVertex>> _tempGraph;

        /// <summary>
        /// Instansiate an object to validate vertex spuds and tees for relations.
        /// <para>
        /// All other elements will be valid by default.
        /// </para>
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="graph"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public VertexRelationValidator(Document doc, IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            _doc = doc;
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _tempGraph = graph.ToBidirectionalGraph();
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

        public bool CheckVertexContainment { get; set; } = false;

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            _validationResults.Clear();
            var vertex = validationContext.ObjectInstance as IVertex;

            if (CheckVertexContainment)
            {
                if (!_graph.TryFindItemByTag(vertex, _doc, out var foundVertex, out var foundEdge) || foundVertex == null)
                {
                    //insert vertex temporary
                    var aGraph = _graph as AdjacencyGraph<IVertex, Edge<IVertex>>;
                    aGraph = aGraph.Clone();
                    var vertexToInsert = vertex is TaggedGVertex<(int, Point3d)> taggedIntPointVertex ?
                        taggedIntPointVertex.ToVertexPoint(aGraph.VertexCount) :
                        vertex;
                    vertex = aGraph.TryInsert(vertexToInsert, _doc);
                    _tempGraph = aGraph.ToBidirectionalGraph();
                }
                else
                {
                    vertex = foundVertex;
                }
            }

            if (vertex == null) { return _validationResults; }

            var famInst = vertex.TryGetFamilyInstance(_doc);
            if (famInst == null || (!famInst.IsSpud() && !famInst.IsTee()))
            { return _validationResults; }

            var (parents, child) = famInst.GetConnectedElements();
            var parentIds = parents is null ? new List<ElementId>() : parents.Select(x => x.Id);

            _tempGraph.TryGetInEdges(vertex, out var inEdges);

            var inElements = vertex.GetInElements(_tempGraph, _doc);
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
