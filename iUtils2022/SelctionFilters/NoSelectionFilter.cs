using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace iUtils.SelctionFilters
{
    public class NoSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
