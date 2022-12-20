using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements
{
    /// <summary>
    /// An object to get geometry elements from document.
    /// </summary>
    public class ElementsExtractor
    {
        private readonly Document _doc;


        /// <summary>
        /// Instantiate an object to get geometry elements from document.
        /// </summary>
        /// <param name="doc"></param>
        public ElementsExtractor(Document doc)
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

        /// <summary>
        /// Get geometry elements from document and all it's loaded links.
        /// </summary>
        /// <returns></returns>
        public (List<Element> elements, Dictionary<RevitLinkInstance, List<Element>> linkElementsDict) GetAll()
        {
            ModelElements = GetFromDoc();
            LinkElements = GetFromLinks();

            return (ModelElements, LinkElements);
        }

        /// <summary>
        /// Get geometry elements from document.
        /// </summary>
        /// <returns></returns>
        public List<Element> GetFromDoc()
        {
            return _doc.GetElements();
        }

        /// <summary>
        /// Get elements from all loaded links in <see cref="Document"/>.
        /// </summary>
        /// <returns>Retruns null if no loaded links are in document.</returns>
        public Dictionary<RevitLinkInstance, List<Element>> GetFromLinks()
        {
            var allLinks = _doc.GetLoadedLinks();
            if (allLinks is null || !allLinks.Any()) return null;

            var elements = new Dictionary<RevitLinkInstance, List<Element>>();
            foreach (var link in allLinks)
            {
                Document linkDoc = link.GetLinkDocument();
                List<Element> geomlinkElems = linkDoc.GetElements(null, link.GetTotalTransform());
                if (geomlinkElems is null || geomlinkElems.Count == 0) { continue; }
                elements.Add(link, geomlinkElems);
            }

            return elements;
        }


    }
}
