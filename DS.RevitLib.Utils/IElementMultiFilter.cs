using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace DS.RevitLib.Utils
{
    /// <summary>
    /// The interface is used to get elements with multi filters. 
    /// </summary>
    public interface IElementMultiFilter
    {
        /// <summary>
        /// Quick filters to get elements.
        /// </summary>
        List<(ElementQuickFilter filter, Func<Transform, ElementQuickFilter> getLinkFilter)> QuickFilters { get; }

        /// <summary>
        /// Slow filters to get elements.
        /// </summary>
        List<(ElementSlowFilter filter, Func<Transform, ElementSlowFilter> getLinkFilter)> SlowFilters { get; }

        /// <summary>
        /// Include only this <see cref="Autodesk.Revit.DB.ElementId"/>s to filter.
        /// </summary>
        List<ElementId> ElementIdsSet { get; set; }

        /// <summary>
        /// Apply filters to active <see cref="Document"/>.
        /// </summary>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.ElementId"/>s from active <see cref="Document"/>.
        /// <para>
        /// Empty <see cref="Autodesk.Revit.DB.ElementId"/>s list if filters are empty.
        /// </para>
        /// </returns>
        IEnumerable<ElementId> ApplyToActiveDoc();

        /// <summary>
        /// Apply filters to all <see cref="RevitLinkInstance"/>s.
        /// </summary>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.ElementId"/>s from all <see cref="RevitLinkInstance"/>s.
        /// <para>
        /// Empty <see cref="Autodesk.Revit.DB.ElementId"/>s list if filters are empty.
        /// </para>
        /// </returns>
        Dictionary<RevitLinkInstance, IEnumerable<ElementId>> ApplyToLinks();

        /// <summary>
        /// Apply filters to active <see cref="Document"/> and all <see cref="RevitLinkInstance"/>s.
        /// </summary>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.ElementId"/>s from active <see cref="Document"/> and all <see cref="RevitLinkInstance"/>s.
        /// <para>
        /// Empty <see cref="Autodesk.Revit.DB.ElementId"/>s list if filters are empty.
        /// </para>
        /// </returns>
        Dictionary<Document, IEnumerable<ElementId>> ApplyToAllDocs();

        /// <summary>
        /// Reset filters.
        /// </summary>
        void Reset();
    }
}