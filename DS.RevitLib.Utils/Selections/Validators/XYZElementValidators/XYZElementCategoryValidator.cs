using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DS.RevitLib.Utils.Selections.Validators
{
    /// <summary>
    /// Validator to check (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>) categories.
    /// </summary>
    public class XYZElementCategoryValidator : IValidator<(Element, XYZ)>, IValidatableObject
    {
        /// <summary>
        /// Current active document.
        /// </summary>
        protected readonly Document _doc;
        private readonly Dictionary<BuiltInCategory, List<PartType>> _availableCategories;
        private readonly List<ValidationResult> _validationResults = new();

        /// <summary>
        /// Instansiate validator to check 
        /// (<see cref="Autodesk.Revit.DB.Element"/>,<see cref="Autodesk.Revit.DB.XYZ"/>) categories.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="availableCategories"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public XYZElementCategoryValidator(Document doc, Dictionary<BuiltInCategory,
            List<PartType>> availableCategories)
        {
            _doc = doc;
            _availableCategories = availableCategories ??
                throw new ArgumentNullException(nameof(availableCategories));
        }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> ValidationResults => _validationResults;

        /// <inheritdoc/>
        public bool IsValid((Element, XYZ) value) =>
            Validate(new ValidationContext(value)).Count() == 0;

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            _validationResults.Clear();
            if (validationContext.ObjectInstance is not ValueTuple<Element, XYZ> pointElement)
            { return _validationResults; }

            if (pointElement.Item1 is FamilyInstance && 
                !pointElement.Item1.IsCategoryElement(_availableCategories))
            { _validationResults.Add(new ValidationResult("Данная категория элемента не доступна для выбора.")); }

            return _validationResults;
        }
    }
}
