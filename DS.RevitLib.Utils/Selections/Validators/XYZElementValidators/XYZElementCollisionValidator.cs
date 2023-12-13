using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DS.RevitLib.Utils.Selections.Validators
{
    /// <summary>
    /// Validator to check (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>) collisions.
    /// </summary>
    public class XYZElementCollisionValidator : IValidator<(Element, XYZ)>, IValidatableObject
    {
        /// <summary>
        /// Current active document.
        /// </summary>
        protected readonly Document _doc;
        private readonly IElementCollisionDetector _elementCollisionDetector;
        private readonly IXYZCollisionDetector _xYZCollisionDetector;
        private readonly List<ValidationResult> _validationResults = new();
        private MEPCurve _baseMEPCurve;

        /// <summary>
        /// Instansiate validator to check (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>) collisions.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elementCollisionDetector"></param>
        /// <param name="xYZCollisionDetector"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public XYZElementCollisionValidator(Document doc,
            IElementCollisionDetector elementCollisionDetector,
            IXYZCollisionDetector xYZCollisionDetector)
        {
            _doc = doc;
            _elementCollisionDetector = elementCollisionDetector ??
                throw new ArgumentNullException(nameof(elementCollisionDetector));
            _xYZCollisionDetector = xYZCollisionDetector;
        }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> ValidationResults => _validationResults;

        /// <summary>
        /// <see cref="MEPCurve"/> to get collisions on point.
        /// </summary>
        public MEPCurve BaseMEPCurve
        {
            get => _baseMEPCurve;
            set
            {
                _baseMEPCurve = value;
                _xYZCollisionDetector.SetMEPCurve(BaseMEPCurve);
            }
        }        

        /// <inheritdoc/>
        public bool IsValid((Element, XYZ) pointElement) =>
            Validate(new ValidationContext(pointElement)).Count() == 0;

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            _validationResults.Clear();
            if (validationContext.ObjectInstance is ValueTuple<Element, XYZ> pointElement)
            {
                var collisions = GetCollisions(pointElement);
                if (collisions is not null && collisions.Any())
                {
                    var report = GetCollisionsReport(collisions);
                    _validationResults.Add(new ValidationResult(report));
                }
            }


            return _validationResults;
        }

        /// <summary>
        /// Specifies whether point has no collisions.
        /// </summary>
        /// <returns>Returns <see langword="true"></see> if point has no collisions.
        /// <para>Otherwise returns <see langword="false"></see>.</para>
        /// </returns>
        private List<(object, Element)> GetCollisions((Element element, XYZ point) pointElement)
        {
            var collisions = new List<(object, Element)>();

            var excludedIds = new List<ElementId>() { pointElement.element.Id };

            _elementCollisionDetector.ExcludedIds = excludedIds;
            switch (pointElement.element)
            {
                case MEPCurve:
                    {
                        var xYZCollisions = _xYZCollisionDetector.
                            GetCollisions(pointElement.point);
                        xYZCollisions.ForEach(c => collisions.Add(c));
                        break;
                    }
                case FamilyInstance:
                    {
                        var connectedMEPCurves =
                                 ConnectorUtils.GetConnectedElements(pointElement.element, true).
                                 OfType<MEPCurve>();
                        excludedIds.AddRange(connectedMEPCurves.Select(e => e.Id));
                        var fcollisions = _elementCollisionDetector.GetCollisions(pointElement.element);
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
