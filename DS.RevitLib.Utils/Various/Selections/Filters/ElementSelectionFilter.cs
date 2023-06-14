using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Various.Selections.Filters;

namespace DS.RevitLib.Utils.SelectionFilters
{
    /// <summary>
    /// Class for filer creation to select only elements of <typeparamref name="T"/> type with geometry.
    /// </summary>
    public class ElementSelectionFilter<T> : IAdvancedSelectionFilter where T : Element
    {
        /// <inheritdoc/>
        public bool AllowLink { get; set; } = true;

        /// <inheritdoc/>
        public bool AllowElement(Element element)
        {
            if (AllowLink && element is RevitLinkInstance) { return true; }
            return element is T && element.IsGeometryElement();
        }

        /// <inheritdoc/>
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
    }
}
