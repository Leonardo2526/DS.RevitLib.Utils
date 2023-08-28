using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Various.Selections.Filters;

namespace DS.RevitLib.Utils.SelectionFilters
{
    /// <summary>
    /// Class for filer creation to select points on specific <see cref="Autodesk.Revit.DB.Element"/>.
    /// Point allowed only on its center line.
    /// </summary>
    public class PointOnElementSelectionFilter : ISelectionFilter
    {
        private readonly Element _elementToSelect;
        private Element _element;

        /// <summary>
        /// Instansiate a filter to select points on line of specific <see cref="Autodesk.Revit.DB.Element"/>.
        /// </summary>
        /// <param name="elementToSelect"></param>
        public PointOnElementSelectionFilter(Element elementToSelect)
        {
            _elementToSelect = elementToSelect;
        }

        /// <inheritdoc/>
        public bool AllowElement(Element element)
        {
            _element = element;
            return element.Id == _elementToSelect.Id;
        }

        /// <inheritdoc/>
        public bool AllowReference(Reference reference, XYZ point)
        {
            return _element.Id == _elementToSelect.Id &&(bool)(_element.GetCenterLine()?.Contains(point));
        }
    }
}
