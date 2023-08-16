using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Connections.PointModels.PointModels;
using DS.RevitLib.Utils.MEP;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            if (!Validator.GetSystemValidity())
            { errors.Add(new ValidationResult("Неверная система объекта.")); }

            var collisions = Validator.GetCollisions();
            if (collisions is not null && collisions.Any())
            { errors.Add(new ValidationResult("Точка не должна располагаться в зоне коллизии.")); }

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
            if(this.Element is null) { IsValid = false; return; }
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
        public XYZ GetDirection(XYZ refPoint, Element refElement, UIDocument uIDocument = null)
        {
            return new ConnectionDirectionFactory(Point, Element, uIDocument).GetDirection(refPoint, refElement);
        }

        public MEPCurve GetMEPCurve(IEnumerable<ElementId> excluededIds = null)
        {
            if(Element is MEPCurve curve) { return  curve; }

            var connectedMEPCurves = ConnectorUtils.GetConnectedElements(Element)?.Where(e => e is MEPCurve);
            if(connectedMEPCurves is null || !connectedMEPCurves.Any()) { return null; }

            if(excluededIds is null || !excluededIds.Any()) {  return connectedMEPCurves.FirstOrDefault() as MEPCurve; }
            var noExeptionIds = connectedMEPCurves.Select(e => e.Id).Except(excluededIds);

            return Element.Document.GetElement(noExeptionIds.FirstOrDefault()) as MEPCurve;
        }
    }
}
