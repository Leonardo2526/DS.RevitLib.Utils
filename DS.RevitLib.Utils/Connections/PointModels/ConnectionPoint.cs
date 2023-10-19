using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Connections.PointModels.PointModels;
using DS.RevitLib.Utils.MEP;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace DS.RevitLib.Utils.Connections.PointModels
{
    /// <summary>
    /// An object that represents connection point of MEPSystem without specifing objects to connect.
    /// </summary>
    public class ConnectionPoint : IConnectionPoint, IValidatableObject
    {
        /// <summary>
        /// Instantiate an object to create connection point of MEPSystem without specifing objects to connect.
        /// </summary>
        ///  <param name="element">Element on connection point.</param>
        /// <param name="point">Joint point</param>
        public ConnectionPoint(Element element, XYZ point)
        {
            Element = element;
            Point = point;
        }

        /// <inheritdoc/>
        public XYZ Point { get; protected set; }

        /// <summary>
        /// Element on connection point.
        /// </summary>
        public Element Element { get; }

        /// <summary>
        /// <see cref="Element"/> partType.
        /// </summary>
        public PartType PartType
        {
            get
            {
                FamilyInstance fam = Element is FamilyInstance ? Element as FamilyInstance : null;
                return fam is null ? PartType.Undefined : ElementUtils.GetPartType(fam);
            }
        }

        /// <inheritdoc/>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Validator for point properties.
        /// </summary>
        public ConnectionPointValidator Validator { get; set; }

        /// <summary>
        /// Direction to connect point.
        /// </summary>
        public XYZ Direction { get; private set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            if (!Validator.GetSystemValidity())
            { errors.Add(new ValidationResult("Неверная система объекта.")); }

            var collisions = Validator.GetCollisions();
            if (collisions is not null && collisions.Any())
            {
                var report = GetCollisionsReport(collisions);
                errors.Add(new ValidationResult(report));
            }

            if (!Validator.IsWithinLengthLimits(Point))
            { errors.Add(new ValidationResult("Точка вне зоны решения коллизии.")); }

            return errors;
        }

        /// <summary>
        /// Specifies whether point is valid for connection.
        /// </summary>
        /// <param name="mEPSystemModel"></param>
        /// <param name="docElements"></param>
        /// <param name="linkElementsDict"></param>
        /// <returns>Returns <see langword="true"></see> if <paramref name="mEPSystemModel"/> contatins point and point has no collisions.
        /// <para>Otherwise returns <see langword="false"></see>.</para>
        /// </returns>
        public void Validate()
        {
            Validator.ConnectionPoint = this;
            if (this.Element is null) { IsValid = false; return; }
            IsValid = Validator.Validate();
        }

        /// <summary>
        /// Get direction by <paramref name="refPoint"/> of <paramref name="refElement"/>.
        /// </summary>
        /// <param name="refPoint"></param>
        /// <param name="refElement"></param>
        /// <param name="uIDocument"></param>
        /// <remarks>
        /// Specify <paramref name="uIDocument"/> if get direction manually should be enabled.
        /// </remarks>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.XYZ"/> direction to connect <see cref="Element"/> at <see cref="Point"/>.
        /// <para>
        /// <see langword="null"/> if no direction was found.
        /// </para>
        /// </returns>
        public XYZ GetDirection(XYZ refPoint, Element refElement, IEnumerable<Element> objectsToExclude = null, UIDocument uIDocument = null)
        {
            var mc = Element is MEPCurve curve ? curve : GetMEPCurve(objectsToExclude.Select(o => o.Id));
            return Direction = new ConnectionDirectionFactory(Point, mc, uIDocument).GetDirection(refPoint, refElement);
        }

        /// <summary>
        /// Get <see cref="MEPCurve"/> to connect.
        /// </summary>
        /// <param name="excluededIds"></param>
        /// <returns></returns>
        public MEPCurve GetMEPCurve(IEnumerable<ElementId> excluededIds = null)
        {
            if (Element is MEPCurve curve) { return curve; }

            var connectedMEPCurves = ConnectorUtils.GetConnectedElements(Element)?.Where(e => e is MEPCurve);
            if (connectedMEPCurves is null || !connectedMEPCurves.Any()) { return null; }

            if (excluededIds is null || !excluededIds.Any()) { return connectedMEPCurves.FirstOrDefault() as MEPCurve; }
            var noExeptionIds = connectedMEPCurves.Select(e => e.Id).Except(excluededIds);

            return Element.Document.GetElement(noExeptionIds.FirstOrDefault()) as MEPCurve;
        }

        private string GetCollisionsReport(List<(XYZ, Element)> collisions)
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
            sb.AppendLine("\nИтого: " + collisions.Count);

            return sb.ToString();
        }
    }
}
