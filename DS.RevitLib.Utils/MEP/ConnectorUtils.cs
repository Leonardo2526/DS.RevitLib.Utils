using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.MainUtils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Neighbours;
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
                    if (elementId != element.Id && MEPElementUtils.CheckMEPElement(con.Owner))
                    {
                        connectedElements.Add(con.Owner);
                    }
                }
            }
            return connectedElements;
        }

        /// <summary>
        /// Get all sysytem elements connected to current element. 
        /// </summary>
        public static List<Element> GetAllConnectedElements(Element element, Document Doc)
        {
            INeighbourSearch neighbourSearch = new Search();
            NeighbourElement neighbourElement = new NeighbourElement(neighbourSearch);
            return neighbourElement.GetAllNeighbours(new List<Element>() { element }, new List<Element>(), Doc);
        }

        /// <summary>
        /// Get all sysytem elements connected to current element which have collisions with sourceColElement. 
        /// </summary>
        public static List<Element> GetAllConnectedWithCollisions(Element element, Element sourceColElement, Document Doc)
        {
            INeighbourSearch neighbourSearch = new SearchWithCollision(new List<Element>() { sourceColElement });
            NeighbourElement neighbourElement = new NeighbourElement(neighbourSearch);
            return neighbourElement.GetAllNeighbours(new List<Element>() { element }, new List<Element>(), Doc);
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
            if (connectorSet is null)
            {
                return new List<Connector>();
            }
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

        public static List<Connector> GetFreeConnector(Element element)
        {
            List<Connector> connectors = GetConnectors(element);
            var freeCons = new List<Connector>();

            foreach (var item in connectors)
            {
                if (!item.IsConnected)
                {
                    freeCons.Add(item);
                }
            }

            return freeCons;
        }

        /// <summary>
        /// Check connectors direction of the element
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Return true if one of directions is equal to input direction. False if it's not.</returns>
        public static bool CheckConnectorsDirection(XYZ direction, Element element)
        {
            List<Connector> connectors = GetConnectors(element);

            foreach (var con in connectors)
            {
                List<Connector> restConnectors = new List<Connector>();
                restConnectors.AddRange(connectors);
                restConnectors.Remove(con);

                foreach (var restcon in restConnectors)
                {
                    if (con.Origin.IsAlmostEqualTo(restcon.Origin))
                    {
                        return true;
                    }
                    Line line = Line.CreateBound(con.Origin, restcon.Origin);
                    if (line.Direction.IsAlmostEqualTo(direction) || line.Direction.Negate().IsAlmostEqualTo(direction))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get common connectors between two elements
        /// </summary>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        /// <returns>Return connectors of element1 and element2</returns>
        public static (Connector elem1Con, Connector elem2Con) GetCommonConnectors(Element element1, Element element2)
        {
            if (!element1.NotNullValidObject() || !element2.NotNullValidObject())
            {
                return (null, null);
            }

            List<Connector> elem1Connectors = GetConnectors(element1);

            foreach (Connector elem1Con in elem1Connectors)
            {
                ConnectorSet connectorSet = elem1Con.AllRefs;

                foreach (Connector con in connectorSet)
                {
                    if (con.Owner.Id == element2.Id && MEPElementUtils.CheckMEPElement(con.Owner))
                    {
                        return (elem1Con, con);
                    }
                }
            }
            return (null, null);
        }

        /// <summary>
        /// Select connector from the list which is closest to baseConnector;
        /// </summary>
        /// <param name="baseConnector"></param>
        /// <param name="connectors"></param>
        /// <returns>Return closest connector.</returns>
        public static Connector GetClosest(Connector baseConnector, List<Connector> connectors)
        {
            Connector resultCon = connectors.FirstOrDefault();
            double distance = baseConnector.Origin.DistanceTo(resultCon.Origin);

            if (connectors.Count > 1)
            {
                for (int i = 1; i < connectors.Count; i++)
                {
                    double curDistance = baseConnector.Origin.DistanceTo(connectors[i].Origin);
                    if (curDistance < distance)
                    {
                        distance = curDistance;
                        resultCon = connectors[i];
                    }
                }
            }

            return resultCon;
        }

        /// <summary>
        /// Select connector from the list which is closest to basePoint;
        /// </summary>
        /// <param name="basePoint"></param>
        /// <param name="connectors"></param>
        /// <returns>Return closest connector.</returns>
        public static Connector GetClosest(XYZ basePoint, List<Connector> connectors)
        {
            Connector resultCon = connectors.FirstOrDefault();
            double distance = basePoint.DistanceTo(resultCon.Origin);

            if (connectors.Count > 1)
            {
                for (int i = 1; i < connectors.Count; i++)
                {
                    double curDistance = basePoint.DistanceTo(connectors[i].Origin);
                    if (curDistance < distance)
                    {
                        distance = curDistance;
                        resultCon = connectors[i];
                    }
                }
            }

            return resultCon;
        }

        public static void ConnectConnectors(Document Doc, Connector c1, Connector c2)
        {
            if (!c1.IsConnectedTo(c2))
            {
                using (Transaction transNew = new Transaction(Doc, "autoMEP_ConnectConnectors"))
                {
                    try
                    {
                        transNew.Start();
                        c1.ConnectTo(c2);
                    }

                    catch (Exception e)
                    {
                        transNew.RollBack();
                        TaskDialog.Show("Revit", e.ToString());
                    }
                    if (transNew.HasStarted())
                    {
                        transNew.Commit();
                    }
                }


            }

        }


        /// <summary>
        /// Get FamilyInstance connected to baseElement by it's connector.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="baseElement"></param>
        /// <returns>Return connected FamilyInstance. Retrun null if no connected FamilyInstances have been found.</returns>
        public static Element GetConnectedByConnector(Connector connector, Element baseElement)
        {
            var connectedElements = new List<Element>();

            ConnectorSet connectorSet = connector.AllRefs;

            foreach (Connector con in connectorSet)
            {
                ElementId elementId = con.Owner.Id;
                Type type = con.Owner.GetType();

                if (elementId != baseElement.Id && type.ToString().Contains("FamilyInstance"))
                    connectedElements.Add(con.Owner);
            }
            if (connectedElements.Count == 0)
            {
                return null;
            }
            return connectedElements.First();
        }

        public static bool DisconnectConnectors(Connector con1, Connector con2)
        {
            Document Doc = con1.Owner.Document;

            using (Transaction transNew = new Transaction(Doc, "DisconnectConnectors"))
            {
                try
                {
                    transNew.Start();
                    con1.DisconnectFrom(con2);
                }

                catch (Exception e)
                { return false; }
                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return true;
        }

        public static bool ConnectConnectors(Connector con1, Connector con2)
        {
            Document Doc = con1.Owner.Document;

            using (Transaction transNew = new Transaction(Doc, "ConnectConnectors"))
            {
                try
                {
                    transNew.Start();
                    con1.ConnectTo(con2);
                }

                catch (Exception e)
                { return false; }
                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return true;
        }
    }
}
