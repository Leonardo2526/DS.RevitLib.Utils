using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Selections.Validators;
using QuickGraph;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Validator to check <see cref="IVertex"/> limits.
    /// </summary>
    public class VertexLimitsValidator : XYZElementLimitsValidator, IValidator<IVertex>
    {
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;

        /// <summary>
        /// Instansiate validator to check <see cref="IVertex"/> limits.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="graph"></param>
        public VertexLimitsValidator(Document doc,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph) : base(doc)
        {
            _graph = graph;
        }

        /// <inheritdoc/>
        public bool IsValid(IVertex value)
        {
            var pointElement = value.ToXYZElement(_doc);
            if (pointElement is (null, null))
            {
                var element = value.TryGetElementFromGraphAndDoc(_graph, _doc);
                if (element is null) { return false; }
                pointElement = (element, value.GetLocation(_doc));
            }
            return Validate(new ValidationContext(pointElement)).Count() == 0;
        }
    }
}
