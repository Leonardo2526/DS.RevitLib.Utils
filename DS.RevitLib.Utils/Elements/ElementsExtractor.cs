using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements
{
    /// <summary>
    /// An object to get geometry elements from document.
    /// </summary>
    public class ElementsExtractor : IElementsExtractor
    {
        private readonly Document _doc;
        private readonly List<BuiltInCategory> _exludedCathegories;
        private readonly Outline _outline;


        /// <summary>
        /// Instantiate an object to get geometry elements from document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="exludedCathegories"></param>
        /// <param name="outline"></param>
        public ElementsExtractor(Document doc, List<BuiltInCategory> exludedCathegories = null, Outline outline = null)
        {
            _doc = doc;
            _exludedCathegories = exludedCathegories;
            _outline = outline;
        }

        /// <summary>
        /// Elements in document.
        /// </summary>
        public List<Element> ModelElements { get; private set; }

        /// <summary>
        /// Elements in all loaded links.
        /// </summary>
        public Dictionary<RevitLinkInstance, List<Element>> LinkElements { get; private set; }

        /// <inheritdoc/>
        public (List<Element> elements, Dictionary<RevitLinkInstance, List<Element>> linkElementsDict) GetAll()
        {
            ModelElements = GetFromDoc();
            LinkElements = GetFromLinks();

            return (ModelElements, LinkElements);
        }

        /// <inheritdoc/>
        public List<Element> GetFromDoc()
        {
            return _doc.GetGeometryElements(null, _exludedCathegories, null, false, _outline);
        }

       /// <inheritdoc/>
        public Dictionary<RevitLinkInstance, List<Element>> GetFromLinks()
        {
            var elements = new Dictionary<RevitLinkInstance, List<Element>>();

            var allLinks = _doc.GetLoadedLinks();
            if (allLinks is null || !allLinks.Any()) return elements;

            foreach (var link in allLinks)
            {
                List<Element> geomlinkElems = _doc.GetGeometryElements(link, _exludedCathegories, null, false, _outline); ;
                if (geomlinkElems is null || geomlinkElems.Count == 0) { continue; }
                elements.Add(link, geomlinkElems);
            }

            return elements;
        }


    }
}
