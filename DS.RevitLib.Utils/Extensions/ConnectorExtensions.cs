using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.Transactions;
using DS.RevitLib.Utils.Various;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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

        /// <summary>
        /// Get elements connected to <paramref name="connector"/> without it's owner.
        /// </summary>
        /// <returns>
        /// List of connected elements. 
        /// Empty list if no connected elements was found.
        /// </returns>
        public static List<Element> GetConnected(this Connector connector, bool includeSubElements = false)
        {
            List<Element> connectedElements = new List<Element>();

            Element owner = connector.Owner;
            Document doc = owner.Document;
            ConnectorSet connectorSet = connector.AllRefs;

            foreach (Connector con in connectorSet)
            {
                ElementId elementId = con.Owner.Id;
                if (elementId != owner.Id && MEPElementUtils.IsValidType(con.Owner))
                {
                    connectedElements.Add(con.Owner);
                    if (includeSubElements && con.Owner is FamilyInstance)
                    {
                        var family = (FamilyInstance)con.Owner;
                        var subElementIds = family.GetSubAllElementIds();
                        subElementIds.ForEach(id => connectedElements.Add(doc.GetElement(id)));
                    }
                }
            }

            return connectedElements;
        }

        /// <summary>
        /// Find <paramref name="element"/> by connected to <paramref name="connector"/> elements without <paramref name="connector"/> owner.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="element"></param>       
        /// <returns>
        /// Found element.
        /// <para>
        /// Othewise returns <see langword="null"/> .
        /// </para>
        /// </returns>
        public static Element Find(this Connector connector, Element element)
        {
            var elements = new ConnectedElementFinder().Find(connector, element);
            return elements.Count > 0 && elements.Last().Id == element.Id ? elements.Last() : null;
        }

        /// <summary>
        /// Find connected to <paramref name="connector"/> elements without <paramref name="connector"/> owner.
        /// </summary>
        /// <param name="connector"></param>
        /// <returns>
        /// Connected elements to <paramref name="connector"/>.
        /// <para>
        /// Empty list if no connected elements was found.
        /// </para>
        /// </returns>
        public static List<Element> Find(this Connector connector) =>
            new ConnectedElementFinder().Find(connector);
    }
}
