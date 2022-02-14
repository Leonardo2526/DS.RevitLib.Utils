using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DS.RevitUtils.MEP
{
    public class ConnectedElement
    {
        private List<Element> ConnectedElements = new List<Element>();

        /// <summary>
        /// Get all elements connected to current element. 
        /// </summary>
        public List<Element> GetConnected(Element element)
        {
            List<Connector> connectors = ConnectorUtils.GetConnectors(element);
            List<Element> connectedElements = new List<Element>();

            foreach (Connector connector in connectors)
            {
                ConnectorSet connectorSet = connector.AllRefs;

                foreach (Connector con in connectorSet)
                {
                    ElementId elementId = con.Owner.Id;
                    if (elementId != element.Id)
                        connectedElements.Add(con.Owner);
                }
            }
            return connectedElements;
        }

        /// <summary>
        /// Get all sysytem elements connected to current element. 
        /// </summary>
        public List<Element> GetAllConnected(Element element, Document Doc)
        {
            return GetAllNeighbours(new List<Element>() { element }, new List<Element>(), Doc);
        }


        private List<Element> GetAllNeighbours(List<Element> elements, List<Element> preElements, Document Doc)
        {
            List<Element> connectedToCurrent = new List<Element>();
            List<Element> elementsForSearch = new List<Element>();
            IEnumerable<ElementId> elementsIds = elements.Select(el => el.Id);
            IEnumerable<ElementId> preElementsIds = preElements.Select(el => el.Id);

            foreach (Element element in elements)
            {
                IEnumerable<ElementId> connectedElemIdsEnum = GetConnected(element).Select(el => el.Id);
                foreach (ElementId elId in connectedElemIdsEnum)
                {
                    if (!preElementsIds.Contains(elId))
                        connectedToCurrent.Add(Doc.GetElement(elId));
                }
            }

            elementsForSearch = GetElementsForSearch(connectedToCurrent);

            if (elementsForSearch.Count > 0)
            {
                ConnectedElements.AddRange(elementsForSearch);
                GetAllNeighbours(elementsForSearch, elements, Doc);
            }

            return ConnectedElements;
        }

        /// <summary>
        /// Get elements for next search step.
        /// </summary>
        private List<Element> GetElementsForSearch(List<Element> elements)
        {
            List<Element> elementsForNewSearch = new List<Element>();

            foreach (Element element in elements)
            {

                Type type = element.GetType();
                if (type.ToString().Contains("System") | type.ToString().Contains("Insulation"))
                    continue;

                elementsForNewSearch.Add(element);

            }

            return elementsForNewSearch;
        }
    }
}
