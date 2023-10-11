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

        /// <summary>
        /// Instantiate an object to get geometry elements from document.
        /// </summary>
        /// <param name="doc"></param>
        public GeometryElementsExtractor(Document doc)
        {
            _doc = doc;
        }

        /// <summary>
        /// Elements in document.
        /// </summary>
        public List<Element> ModelElements { get; private set; }

        /// <summary>
        /// Elements in all loaded links.
        /// </summary>
        public Dictionary<RevitLinkInstance, List<Element>> LinkElements { get; private set; }
        public List<BuiltInCategory> ExludedCathegories { get; set; }
        public Outline Outline { get; set; }

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
            return _doc.GetGeometryElements(null, ExludedCathegories, null, false, Outline);
        }

       /// <inheritdoc/>
        public Dictionary<RevitLinkInstance, List<Element>> GetFromLinks()
        {
            var elements = new Dictionary<RevitLinkInstance, List<Element>>();

            var allLinks = _doc.GetLoadedLinks();
            if (allLinks is null || !allLinks.Any()) return elements;

            foreach (var link in allLinks)
            {
                List<Element> geomlinkElems = _doc.GetGeometryElements(link, ExludedCathegories, null, false, Outline); ;
                if (geomlinkElems is null || geomlinkElems.Count == 0) { continue; }
                elements.Add(link, geomlinkElems);
            }

            return elements;
        }


    }
}
