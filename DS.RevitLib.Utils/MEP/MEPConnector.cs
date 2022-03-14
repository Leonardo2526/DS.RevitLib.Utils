using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP
{
    public static class MEPConnector
    {
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
    }
}
