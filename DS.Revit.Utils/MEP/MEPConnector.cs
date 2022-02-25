using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace DS.Revit.Utils.MEP
{
    static class MEPConnector
    {
        public static void GetCommonConnector(out Connector con1, out Connector con2, 
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

        public static List<Connector> GetMEPConnectors(MEPCurve mepCurve)
        {
            //1. Get connector set of MEPCurve
            ConnectorSet connectorSet = mepCurve.ConnectorManager.Connectors;

            //2. Initialise empty list of connectors
            List<Connector> connectorList = new List<Connector>();

            //3. Loop through connector set and add to list
            foreach (Connector connector in connectorSet)
            {
                connectorList.Add(connector);
            }
            return connectorList;
        }

        public static List<Connector> GetConnectors(Element element)
        {
            //1. Cast Element to FamilyInstance
            FamilyInstance inst = element as FamilyInstance;

            //2. Get MEPModel Property
            MEPModel mepModel = inst.MEPModel;

            //3. Get connector set of MEPModel
            ConnectorSet connectorSet = mepModel.ConnectorManager.Connectors;

            //4. Initialise empty list of connectors
            List<Connector> connectorList = new List<Connector>();

            //5. Loop through connector set and add to list
            foreach (Connector connector in connectorSet)
            {
                connectorList.Add(connector);
            }
            return connectorList;
        }

        public static List<XYZ> GetConnectorsXYZ(MEPCurve mepCurve)
        {
            ConnectorSet connectorSet = mepCurve.ConnectorManager.Connectors;
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
