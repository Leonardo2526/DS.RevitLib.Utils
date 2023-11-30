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

namespace DS.RevitLib.Utils.Selections.Validators
{
    /// <summary>
    /// Validator to check (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>) limits.
    /// </summary>
    public class XYZElementLimitsValidator : IValidator<(Element, XYZ)>, IValidatableObject
    {
        /// <summary>
        /// Current active document.
        /// </summary>
        protected readonly Document _doc;
        private readonly List<ValidationResult> _validationResults = new();

        /// <summary>
        /// Instansiate validator to check 
        /// (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>) limits.
        /// </summary>
        /// <param name="doc"></param>
        public XYZElementLimitsValidator(Document doc)
        {
            _doc = doc;
        }


        #region Properties

        /// <summary>
        /// Vertex bound of <see cref="Document"/>.
        /// </summary>
        public Outline BoundOutline { get; set; }


        /// <summary>
        /// Specifies whether allow insulation account or not.
        /// </summary>
        public bool IsInsulationAccount { get; set; }

        /// <summary>
        /// Minimimum distance from <see cref="Autodesk.Revit.DB.Element"/>'s bottom or its insulation to floor.
        /// </summary>
        public double MinDistToFloor { get; set; }

        /// <summary>
        /// Minimimum distance from <see cref="Autodesk.Revit.DB.Element"/>'s top or its insulation to ceiling.
        /// </summary>
        public double MinDistToCeiling { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> ValidationResults => _validationResults;

        #endregion

        /// <inheritdoc/>
        public bool IsValid((Element,  XYZ ) pointElement) =>
            Validate(new ValidationContext(pointElement)).Count() == 0;

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (validationContext.ObjectInstance is not ValueTuple<Element, XYZ> pointElement)
            { return results; }

            if (!IsWithinOutlineLimits(BoundOutline, pointElement))
            { results.Add(new ValidationResult("Vertex is outside bound limits.")); }

            if (!IsWithinFloorsBounds(pointElement, _doc, MinDistToFloor, MinDistToCeiling, IsInsulationAccount))
            { results.Add(new ValidationResult("Excluded types contains vertex.")); }

            return results;
        }


        #region PrivateMethods

        private bool IsWithinOutlineLimits(Outline boundOutline, (Element element, XYZ point) pointElement)
        {
            if (boundOutline == null) { return true; }

            var point = pointElement.point;
            return point.More(boundOutline.MinimumPoint) && point.Less(boundOutline.MaximumPoint);

        }


        private bool IsWithinFloorsBounds((Element element, XYZ point) pointElement, Document doc,
        double minDistToFloor, double minDistToCeiling,
        bool isInsulationAccount, int distnaceToFindFloor = 30)
        {
            if (minDistToFloor == 0 && minDistToCeiling == 0) { return true; }

            (XYZ pointFloorBound, XYZ pointCeilingBound) = pointElement.GetFloorBounds(
                doc, minDistToFloor, minDistToCeiling, 
                isInsulationAccount, distnaceToFindFloor);

            return pointFloorBound != null && pointCeilingBound != null;
        }


        #endregion

    }
}
