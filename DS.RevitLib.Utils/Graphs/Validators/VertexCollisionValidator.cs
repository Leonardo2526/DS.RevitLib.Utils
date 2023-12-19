using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Selections.Validators;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DS.RevitLib.Utils.Graphs.Validators
{
    /// <summary>
    /// Validator to check <see cref="IVertex"/> collisions.
    /// </summary>
    public class VertexCollisionValidator : XYZElementCollisionValidator, IValidator<IVertex>
    {
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;


        /// <summary>
        /// Instansiate validator to check <see cref="IVertex"/> collisions.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elementCollisionDetector"></param>
        /// <param name="xYZCollisionDetector"></param>
        /// <param name="graph"></param>
        public VertexCollisionValidator(Document doc,
            IElementCollisionDetector elementCollisionDetector,
            IXYZCollisionDetector xYZCollisionDetector,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph) :
            base(doc, elementCollisionDetector, xYZCollisionDetector)
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
                if(element is null) { return false; }
                pointElement = (element, value.GetLocation(_doc));
            }
            return Validate(new ValidationContext(pointElement)).Count() == 0;
        }
    }
}
