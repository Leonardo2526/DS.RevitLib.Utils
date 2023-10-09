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
        /// Get <see cref="Autodesk.Revit.DB.Element"/>'s from active <see cref="Document"/> and all it's loaded links.
        /// </summary>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Element"/>'s in active <see cref="Document"/> and links. 
        /// <para>
        /// Returns empty collecions if document don't contains elements or <see cref="RevitLinkInstance"/>'s.
        /// </para> 
        /// </returns>
        (List<Element> elements, Dictionary<RevitLinkInstance, List<Element>> linkElementsDict) GetAll();

        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.Element"/>'s from active <see cref="Document"/>.
        /// </summary>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Element"/>'s from active <see cref="Document"/>.
        /// </returns>
        List<Element> GetFromDoc();

        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.Element"/>'s  from all loaded links in <see cref="Document"/>.
        /// </summary>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Element"/>'s from all loaded links.
        /// <para>
        /// Empty list if no loaded links exist in active <see cref="Document"/>.
        /// </para>
        /// </returns>
        Dictionary<RevitLinkInstance, List<Element>> GetFromLinks();
    }
}