using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitUtils.MEP
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

      

       
    }
}
