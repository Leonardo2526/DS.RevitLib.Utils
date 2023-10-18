using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements
{
    /// <summary>
    /// An object to get geometry elements from document.
    /// </summary>
    public class GeometryElementsExtractor : IElementsExtractor
    {
        private readonly Document _doc;
        private List<Element> _activeDocElements;
        private List<(RevitLinkInstance, Transform, List<Element>)> _linkElements;
        private List<BuiltInCategory> _exludedCategories;
        private Outline _outline;

        /// <summary>
        /// Instantiate an object to get geometry elements from document.
        /// </summary>
        /// <param name="doc"></param>
        public GeometryElementsExtractor(Document doc)
        {
            _doc = doc;
        }

        /// <inheritdoc/>
        public List<Element> ActiveDocElements
        { get => _activeDocElements ??= GetFromDoc(); set => _activeDocElements = value; }

        /// <inheritdoc/>
        public List<(RevitLinkInstance, Transform, List<Element>)> LinkElements
        { get => _linkElements ??= GetFromLinks(); set => _linkElements = value; }

        /// <summary>
        /// Categories to exclude from extraction result.
        /// <para>
        /// Setting of this property will clear <see cref="ActiveDocElements"/> and <see cref="LinkElements"/>.
        /// </para>
        /// </summary>
        public List<BuiltInCategory> ExludedCategories
        {
            get => _exludedCategories;
            set { _exludedCategories = value; ClearElements(); }
        }

        /// <summary>
        /// Only elements inside this outline will be include to extraction result.
        /// <para>
        /// Setting of this property will clear <see cref="ActiveDocElements"/> and <see cref="LinkElements"/>.
        /// </para>
        /// </summary>
        public Outline Outline
        {
            get => _outline;
            set { _outline = value; ClearElements(); }
        }

        /// <inheritdoc/>
        private List<Element> GetFromDoc()
        {
            return _doc.GetGeometryElements(null, ExludedCategories, null, false, Outline);
        }

        /// <inheritdoc/>
        private List<(RevitLinkInstance, Transform, List<Element>)> GetFromLinks()
        {
            var elements = new List<(RevitLinkInstance, Transform, List<Element>)>();

            var allLinks = _doc.GetLoadedLinks();
            if (allLinks is null || !allLinks.Any()) return elements;

            foreach (var link in allLinks)
            {
                List<Element> geomlinkElems = _doc.GetGeometryElements(link, ExludedCategories, null, false, Outline);
                if (geomlinkElems is null || geomlinkElems.Count == 0) { continue; }
                var model = (link, link.GetLinkTransform(), geomlinkElems);
                elements.Add(model);
            }

            return elements;
        }

        private void ClearElements()
        {
            _activeDocElements = null;
            _linkElements = null;
        }

    }
}
