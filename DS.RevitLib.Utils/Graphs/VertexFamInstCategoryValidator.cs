using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Extensions;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// The object to validate vertex categories.
    /// </summary>
    public class VertexFamInstCategoryValidator : IValidator<IVertex>, IValidatableObject
    {
        private readonly Document _doc;
        private readonly Dictionary<BuiltInCategory, List<PartType>> _availableCategories;

        /// <summary>
        /// Instansiate object to validate vertex categories.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="availableCategories"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public VertexFamInstCategoryValidator(Document doc, Dictionary<BuiltInCategory,
            List<PartType>> availableCategories)
        {
            _doc = doc;
            _availableCategories = availableCategories ??
                throw new ArgumentNullException(nameof(availableCategories));
        }

        /// <inheritdoc/>
        public bool IsValid(IVertex value) =>
            Validate(new ValidationContext(value)).Count() == 0;

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            var vertex = validationContext.ObjectInstance as IVertex;

            var famInst = vertex.TryGetFamilyInstance(_doc);
            if (famInst == null)
            { return results; }

            if (!famInst.IsCategoryElement(_availableCategories))
            { results.Add(new ValidationResult("Vertex category is not allow.")); }

          

            return results;
        }
    }
}
