using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Neighbours;
using System.Collections.Generic;
using System.Linq;


namespace DS.RevitLib.Utils.MEP
{
    internal class NeighbourElement
    {
        INeighbourSearch NeighbourSearch;

        public NeighbourElement(INeighbourSearch neighbourSearch)
        {
            NeighbourSearch = neighbourSearch;
        }

        private List<Element> ConnectedElements = new List<Element>();

        public List<Element> GetAllNeighbours(List<Element> elements, List<Element> excludedElements, Document Doc)
        {
            List<Element> connectedToCurrent = new List<Element>();
            List<Element> elementsForSearch = new List<Element>();
            IEnumerable<ElementId> elementsIds = elements.Select(el => el.Id);
            IEnumerable<ElementId> excludedElementsIds = excludedElements.Select(el => el.Id);

            foreach (Element element in elements)
            {
                IEnumerable<ElementId> connectedElemIdsEnum = ConnectorUtils.GetConnectedElements(element).Select(el => el.Id);
                foreach (ElementId elId in connectedElemIdsEnum)
                {
                    if (!excludedElementsIds.Contains(elId))
                        connectedToCurrent.Add(Doc.GetElement(elId));
                }
            }

            elementsForSearch = NeighbourSearch.GetElementsForSearch(connectedToCurrent);

            if (elementsForSearch.Count > 0)
            {
                ConnectedElements.AddRange(elementsForSearch);
                GetAllNeighbours(elementsForSearch, elements, Doc);
            }

            return ConnectedElements;
        }
    }
}
