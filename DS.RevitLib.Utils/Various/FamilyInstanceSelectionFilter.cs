using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Extensions;

namespace DS.RevitLib.Utils.SelectionFilters
{
    /// <summary>
    /// Class for filer creation to select only family instances with geometry.
    /// </summary>
    public class FamilyInstanceSelectionFilter : ISelectionFilter
    {
        /// <inheritdoc/>
        public bool AllowElement(Element element)
        {
            if (element is FamilyInstance && element.IsGeometryElement())
            {
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
    }
}
