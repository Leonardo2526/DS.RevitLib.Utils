using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Elements
{
    /// <summary>
    /// The interface is used to get elements from <see cref="Document"/> and it's <see cref="RevitLinkInstance"/>'s.
    /// </summary>
    public interface IElementsExtractor
    {

        /// <summary>
        /// Elements in document.
        /// </summary>
        public List<Element> ActiveDocElements { get; set; }

        /// <summary>
        /// Elements in all loaded links.
        /// </summary>
        public List<(RevitLinkInstance, Transform, List<Element>)> LinkElements { get; }

        /// <summary>
        /// Categories to exclude from extraction result.
        /// </summary>
        List<BuiltInCategory> ExludedCategories { get; set; }

        /// <summary>
        /// Only elements inside this outline will be include to extraction result.
        /// </summary>
        Outline Outline { get; set; }


    }
}