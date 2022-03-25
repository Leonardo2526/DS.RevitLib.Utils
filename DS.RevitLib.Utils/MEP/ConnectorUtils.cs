using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP
{
    public static class ConnectorUtils
    {

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
                    if (elementId != element.Id && CheckMEPElement(con.Owner))
                    {
                        connectedElements.Add(con.Owner);
                    }
                }
            }
            return connectedElements;
        }

        /// <summary>
        /// Check connected elements for type
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Return true if element is not System or Insulation type</returns>
        private static bool CheckMEPElement(Element element)
        {
            Type type = element.GetType();
            if (type.ToString().Contains("System") | type.ToString().Contains("Insulation"))
            {
                return false;
            }

            return true;
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
                if (CheckMEPElement(element))
                {
                    elementsForNewSearch.Add(element);
                }
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

        public static List<Element> GetConnectedWithExclusions(Element sourceElement, List<Element> excludedElements = null)
        {
            List<Element> elements = GetConnectedElements(sourceElement);

            var NoIntersections = new List<Element>();

            foreach (var one in elements)
            {
                if (!excludedElements.Any(two => two.Id == one.Id))
                {
                    NoIntersections.Add(one);
                }
            }

            return NoIntersections;
        }

        public static void GetNeighbourConnectors(out Connector con1, out Connector con2,
        List<Connector> connectors1, List<Connector> connectors2)
        {
            con1 = null;
            con2 = null;
            foreach (Connector connector1 in connectors1)
            {
                foreach (Connector connector2 in connectors2)
                {
                    if (Math.Abs(connector1.Origin.X - connector2.Origin.X) < 0.01 &&
                        Math.Abs(connector1.Origin.Y - connector2.Origin.Y) < 0.01 &&
                        Math.Abs(connector1.Origin.Z - connector2.Origin.Z) < 0.01)
                    {
                        con1 = connector1;
                        con2 = connector2;
                        break;
                    }
                }
            }

        }

        public static List<Connector> GetConnectors(Element element)
        {
            ConnectorSet connectorSet = GetConnectorSet(element);

            //Initialise empty list of connectors
            List<Connector> connectorList = new List<Connector>();

            //Loop through connector set and add to list
            foreach (Connector connector in connectorSet)
            {
                connectorList.Add(connector);
            }
            return connectorList;
        }

        /// <summary>
        /// Get connectorSet of elemetn. Return new connectorSet if element is FamilyInstance or MEPCurve.
        /// Return null if it isn't;
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static ConnectorSet GetConnectorSet(Element element)
        {
            if (element.GetType().Name == "FamilyInstance")
            {
                //Cast Element to FamilyInstance
                FamilyInstance inst = element as FamilyInstance;

                //Get MEPModel Property
                MEPModel mepModel = inst.MEPModel;

                //Get connector set of MEPModel
                return mepModel.ConnectorManager.Connectors;
            }
            else if (ElementUtils.IsElementMEPCurve(element))
            {
                MEPCurve mepCurve = element as MEPCurve;

                //Get connector set of MEPModel
                return mepCurve.ConnectorManager.Connectors;
            }

            return null;
        }

        public static List<XYZ> GetConnectorsXYZ(Element element)
        {
            ConnectorSet connectorSet = GetConnectorSet(element);
            List<XYZ> connectorPointList = new List<XYZ>();
            foreach (Connector connector in connectorSet)
            {
                XYZ connectorPoint = connector.Origin;
                connectorPointList.Add(connectorPoint);
            }

            return connectorPointList;
        }

        public static double GetLengthBetweenConnectors(Connector c1, Connector c2)
        {
            return c1.Origin.DistanceTo(c2.Origin);
        }

        public static (Connector freeConnector, Connector attachedConnector) GetConnectorsByAttach(Element element)
        {
            Connector freeConnector = null;
            Connector attachedConnector = null;

            List<Connector> connectors = GetConnectors(element);

            foreach (var item in connectors)
            {
                if (item.IsConnected)
                {
                    attachedConnector = item;
                }
                else
                {
                    freeConnector = item;
                }
            }

            return (freeConnector, attachedConnector);
        }

        public static (Connector con1, Connector con2) GetCommonConnectors(Element element1, Element element2)
        {
            List<Connector> element1Connectors = GetConnectors(element1);
            List<Connector> element2Connectors = GetConnectors(element2);

            GetNeighbourConnectors(out Connector con1, out Connector con2, element1Connectors, element2Connectors);

            return (con1, con2);
        }
    }
}
