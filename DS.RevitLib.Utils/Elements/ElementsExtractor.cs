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
        private readonly List<BuiltInCategory> _exludedCathegories;


        /// <summary>
        /// Instantiate an object to get geometry elements from document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="exludedCathegories"></param>
        public ElementsExtractor(Document doc, List<BuiltInCategory> exludedCathegories = null)
        {
            _doc = doc;
            _exludedCathegories = exludedCathegories;
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
        /// <returns>
        /// Elements in document and links. 
        /// <para>
        /// Returns empty collecions if document don't contains elements or <see cref="RevitLinkInstance"/>'s.
        /// </para> 
        /// </returns>
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
            return _doc.GetGeometryElements(_exludedCathegories);
        }

        /// <summary>
        /// Get elements from all loaded links in <see cref="Document"/>.
        /// </summary>
        /// <returns>Retruns empty list if no loaded links are in document.</returns>
        public Dictionary<RevitLinkInstance, List<Element>> GetFromLinks()
        {
            var elements = new Dictionary<RevitLinkInstance, List<Element>>();

            var allLinks = _doc.GetLoadedLinks();
            if (allLinks is null || !allLinks.Any()) return elements;

            foreach (var link in allLinks)
            {
                Document linkDoc = link.GetLinkDocument();
                List<Element> geomlinkElems = linkDoc.GetGeometryElements(_exludedCathegories);
                if (geomlinkElems is null || geomlinkElems.Count == 0) { continue; }
                elements.Add(link, geomlinkElems);
            }

            return elements;
        }


    }
}
