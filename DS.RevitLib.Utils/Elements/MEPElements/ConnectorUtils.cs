using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
        /// <returns>Returns list of connected elements. 
        /// Returns empty list if no connected elements was found </returns>
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
                    if (elementId != element.Id && MEPElementUtils.IsValidType(con.Owner))
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
        public static List<Element> GetAllConnectedElements(Element element, Document Doc, List<Element> exculdedElements = null)
        {
            INeighbourSearch neighbourSearch = new Search();
            NeighbourElement neighbourElement = new NeighbourElement(neighbourSearch);
            return neighbourElement.GetAllNeighbours(new List<Element>() { element }, exculdedElements ?? new List<Element>(), Doc);
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
                if ((bool)!excludedElements?.Any(two => two.Id == one.Id))
                {
                    NoIntersections.Add(one);
                }
            }

            return NoIntersections;
        }

        /// <summary>
        /// Get all sysytem elements connected to current element. 
        /// </summary>
        public static List<Element> GetConnectedElements(Element element, Document Doc)
        {
            INeighbourSearch neighbourSearch = new Search();
            NeighbourElement neighbourElement = new NeighbourElement(neighbourSearch);
            return neighbourElement.GetAllNeighbours(new List<Element>() { element }, new List<Element>(), Doc);
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

        /// <summary>
        /// Get neighbour connectors. 
        /// </summary>
        /// <param name="connectors1"></param>
        /// <param name="connectors2"></param>
        /// <returns>Returns two connectors of elements with zero distance.</returns>
        public static (Connector con1, Connector con2) GetNeighbourConnectors(List<Connector> connectors1, List<Connector> connectors2)
        {
            Connector elem1Con = null, elem2Con = null;
            foreach (Connector c1 in connectors1)
            {
                var cons2 = connectors2.
                    Where(con => (con.Origin - c1.Origin).IsZeroLength());
                if (!cons2.Any())
                {
                    continue;
                }
                elem1Con = c1;
                elem2Con = cons2.First();
                break;
            }

            return (elem1Con, elem2Con);
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
            else if (element is MEPCurve)
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
        /// Get common connected connectors between two elements
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
                    if (con.Owner.Id == element2.Id && MEPElementUtils.IsValidType(con.Owner))
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

        /// <summary>
        /// Select connector from the list which is closest to line;
        /// </summary>
        /// <param name="line"></param>
        /// <param name="connectors"></param>
        /// <returns>Return closest connector.</returns>
        public static Connector GetClosest(Line line, List<Connector> connectors)
        {
            Connector resultCon = connectors.FirstOrDefault();
            double distance = line.Distance(resultCon.Origin);

            if (connectors.Count > 1)
            {
                for (int i = 1; i < connectors.Count; i++)
                {
                    double curDistance = line.Distance(connectors[i].Origin);
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
        /// Get two connector from the lists with minimum distance between them.
        /// </summary>
        /// <param name="connectors1"></param>
        /// <param name="connectors2"></param>
        /// <returns>Return closest connectors.</returns>
        public static (Connector con1, Connector con2) GetClosest(List<Connector> connectors1, List<Connector> connectors2)
        {
            Connector resCon1 = null;
            Connector resCon2 = null;
            double distance = 10000;

            foreach (var c1 in connectors1)
            {
                foreach (var c2 in connectors2)
                {
                    double curDistance = c1.Origin.DistanceTo(c2.Origin);
                    if (curDistance < distance)
                    {
                        distance = curDistance;
                        resCon1 = c1;
                        resCon2 = c2;
                    }
                }
            }

            return (resCon1, resCon2);
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

        /// <summary>
        /// Check if elements are connected.
        /// </summary>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        /// <returns>Return true if elements are connected.</returns>
        public static bool ElementsConnected(Element element1, Element element2)
        {
            var (elem1Con, elem2Con) = GetCommonConnectors(element1, element2);
            if (elem1Con is null || elem2Con is null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get main connectors of element. 
        /// </summary>
        /// <param name="element"></param>
        /// <returns>If element is MEPCurve returns two connectors of it with max distance between them.
        /// If element is FamilyInstance returns two connectors if element's location point is on line between them.</returns>
        public static (Connector con1, Connector con2) GetMainConnectors(Element element)
        {
            var connectors = GetConnectors(element);
            XYZ lp = ElementUtils.GetLocationPoint(element);

            if (element is FamilyInstance)
            {
                if(connectors.Count ==2) { return (connectors[0], connectors[1]); }
                for (int i = 0; i < connectors.Count - 1; i++)
                {
                    for (int j = i + 1; j < connectors.Count; j++)
                    {
                        XYZ dir1 = (connectors[i].Origin - lp).RoundVector();
                        XYZ dir2 = (connectors[j].Origin - lp).RoundVector();
                        if (XYZUtils.Collinearity(dir1, dir2))
                        {
                            return (connectors[i], connectors[j]);
                        }
                    }
                }
            }
            else if (element is MEPCurve)
            {
                List<XYZ> points = connectors.Select(obj => obj.Origin).ToList();
                var (point1, point2) = XYZUtils.GetMaxDistancePoints(points, out double dist);

                var con1 = connectors.Where(c => Math.Round(c.Origin.DistanceTo(point1), 3) == 0).First();
                var con2 = connectors.Where(c => Math.Round(c.Origin.DistanceTo(point2), 3) == 0).First();
                return (con1, con2);
            }


            return (null, null);
        }

        /// <summary>
        /// Get common not connected connectors between two elements
        /// </summary>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        /// <returns>Reruns two connecors with the same origin poin.</returns>
        public static (Connector con1, Connector con2) GetCommonNotConnectedConnectors(Element element1, Element element2)
        {
            var cons1 = GetConnectors(element1);
            var cons2 = GetConnectors(element2);

            foreach (var c1 in cons1)
            {
                foreach (var c2 in cons2)
                {
                    if ((c1.Origin - c2.Origin).IsZeroLength()) { return (c1, c2); }
                }
            }

            return (null, null);
        }
    }
}
