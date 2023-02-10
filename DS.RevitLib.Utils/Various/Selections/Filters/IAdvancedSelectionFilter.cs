using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace DS.RevitLib.Utils.Various.Selections.Filters
{
    ///<inheritdoc/>
    public interface IAdvancedSelectionFilter : ISelectionFilter
    {
        /// <summary>
        /// Specify if filter is allowed to select <see cref="RevitLinkInstance"/>.
        /// </summary>
        bool AllowLink { get; set; }
    }
}
