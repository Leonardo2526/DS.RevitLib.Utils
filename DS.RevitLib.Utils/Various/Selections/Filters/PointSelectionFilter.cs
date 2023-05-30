using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Various.Selections.Filters;

namespace DS.RevitLib.Utils.SelectionFilters
{
    /// <summary>
    /// Class for filer creation to select only elements of <typeparamref name="T"/> type with geometry.
    /// Point allowed only on its center line.
    /// </summary>
    public class PointSelectionFilter<T> : IAdvancedSelectionFilter where T : Element
    {
        private Element _element;

        /// <inheritdoc/>
        public bool AllowLink { get; set; } = true;

        /// <inheritdoc/>
        public bool AllowElement(Element element)
        {
            _element = element;
            if (AllowLink && element is RevitLinkInstance) { return true; }
            return element is T && element.IsGeometryElement();
        }

        /// <inheritdoc/>
        public bool AllowReference(Reference reference, XYZ point)
        {
            return (bool)(_element.GetCenterLine()?.Contains(point));
        }
    }
}
