using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Object representing extension methods to work with <see cref="Autodesk.Revit.DB.Connector"/>.
    /// </summary>
    public static class ConnectorExtensions
    {
        /// <summary>
        /// Get elements that contain current <paramref name="connector"/>.
        /// </summary>
        /// <param name="connector"></param>
        /// <returns>Returns elements that contain current <paramref name="connector"/>.
        /// Otherwise returns empty list.</returns>
        public static List<Element> GetOwners(this Connector connector)
        {
            List<Element> connectedElements = new();

            ConnectorSet connectorSet = connector.AllRefs;
            foreach (Connector con in connectorSet)
            {
                if (MEPElementUtils.IsValidType(con.Owner))
                {
                    connectedElements.Add(con.Owner);
                }
            }

            return connectedElements;
        }
    }
}
