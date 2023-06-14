using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Connections.PointModels.PointModels;
using DS.RevitLib.Utils;
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
            IsValid = Validator.Validate();
        }
    }
}
