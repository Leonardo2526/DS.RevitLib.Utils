using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.Revit.Utils.MEP
{
    public class ConnectorUtils
    {

        public static List<Connector> GetConnectors(Element element)
        {
            //1. Get connector set
            ConnectorSet connectorSet = null;

            Type type = element.GetType();

            if (type.ToString().Contains("FamilyInstance"))
            {
                FamilyInstance familyInstance = element as FamilyInstance;
                connectorSet = familyInstance.MEPModel.ConnectorManager.Connectors;
            }
            else
            {
                try
                {
                    MEPCurve mepCurve = element as MEPCurve;
                    connectorSet = mepCurve.ConnectorManager.Connectors;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                }

            }

            //2. Initialise empty list of connectors
            List<Connector> connectorList = new List<Connector>();

            //3. Loop through connector set and add to list
            foreach (Connector connector in connectorSet)
            {
                connectorList.Add(connector);
            }
            return connectorList;
        }

        /// <summary>
        /// Get elements connected to current element. 
        /// </summary>
        public static List<Element> GetConnectedElements(Element element)
        {
            List<Connector> connectors = GetConnectors(element);
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


        private static List<Element> ConnectedElements = new List<Element>();

        /// <summary>
        /// Get all sysytem elements connected to current element. 
        /// </summary>
        public static List<Element> GetAllConnectedElements(Element element, Document Doc)
        {
            return GetAllNeighbours(new List<Element>() { element }, new List<Element>(), Doc);
        }

        private static List<Element> GetAllNeighbours(List<Element> elements, List<Element> preElements, Document Doc)
        {
            List<Element> connectedToCurrent = new List<Element>();
            List<Element> elementsForSearch = new List<Element>();
            IEnumerable<ElementId> elementsIds = elements.Select(el => el.Id);
            IEnumerable<ElementId> preElementsIds = preElements.Select(el => el.Id);

            foreach (Element element in elements)
            {
                IEnumerable<ElementId> connectedElemIdsEnum = GetConnectedElements(element).Select(el => el.Id);
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
        private static List<Element> GetElementsForSearch(List<Element> elements)
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

        public static List<Element> GetConnectedFamilyInstances(Element element)
        {
            List<Connector> connectors = GetConnectors(element);

            List<Element> connectedElements = new List<Element>();
            foreach (Connector connector in connectors)
            {
                ConnectorSet connectorSet = connector.AllRefs;

                foreach (Connector con in connectorSet)
                {
                    ElementId elementId = con.Owner.Id;
                    Type type = con.Owner.GetType();
                    if (elementId != element.Id && type.ToString().Contains("FamilyInstance"))
                        connectedElements.Add(con.Owner);
                }
            }
            return connectedElements;
        }

    }
}
