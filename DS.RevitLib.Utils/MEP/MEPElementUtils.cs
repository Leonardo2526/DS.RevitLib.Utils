using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.TransactionCommitter;
using Ivanov.RevitLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP
{
    public static class MEPElementUtils
    {
        public static XYZ GetElbowCenterPoint(FamilyInstance familyInstance)
        {
            BuiltInCategory familyInstanceCategory = CategoryExtension.GetBuiltInCategory(familyInstance.Category);

            List<BuiltInCategory> builtInCategories = new List<BuiltInCategory>
            { BuiltInCategory.OST_PipeFitting, BuiltInCategory.OST_DuctFitting};

            if (!ElementUtils.CheckCategory(familyInstanceCategory, builtInCategories))
            {
                TaskDialog.Show("GetElbowCenterPoint", "Error occured! Element is not fitting.");
                return null;
            }

            List<Connector> connectors = ConnectorUtils.GetConnectors(familyInstance);

            XYZ c0 = connectors[0].CoordinateSystem.BasisZ;
            Line line0 = Line.CreateBound(connectors[0].Origin + c0, connectors[0].Origin);
            line0 = line0.IncreaseLength(10);

            XYZ c1 = connectors[1].CoordinateSystem.BasisZ;
            Line line1 = Line.CreateBound(connectors[1].Origin + c1, connectors[1].Origin);
            line1 = line1.IncreaseLength(10);

            var res = line0.Intersect(line1, out IntersectionResultArray resultArray);
            XYZ intersectionPoint = resultArray.get_Item(0).XYZPoint;

            return intersectionPoint;
        }

        /// <summary>
        /// Get list of all directions of element's connectors
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static List<XYZ> GetDirection(Element element)
        {
            List<XYZ> direction = new List<XYZ>();

            List<Connector> connectors = ConnectorUtils.GetConnectors(element);
            Connector defaultCon = connectors.FirstOrDefault();

            foreach (var con in connectors)
            {
                if (defaultCon.Origin.IsAlmostEqualTo(con.Origin))
                {
                    continue;
                }
                Line line = Line.CreateBound(defaultCon.Origin, con.Origin);
                direction.Add(line.Direction);
            }

            return direction;
        }

        public static XYZ GetCenterPoint(Element element)
        {
            List<Connector> connectors = ConnectorUtils.GetConnectors(element);

            if (connectors.Count == 2)
            {
                return GetElbowCenterPoint(element as FamilyInstance);
            }
            else
            {
                return ElementUtils.GetLocationPoint(element);
            }
        }

        /// <summary>
        /// Check connected elements for type
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Return true if element is not System or Insulation type</returns>
        public static bool IsValidType(Element element)
        {
            Type type = element.GetType();

            if (type.Name.Contains("System") | type.Name.Contains("Insulation"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get element's parameter associated with parameter of connectors.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="connectorParameter"></param>
        /// <returns>Return assiciated parameter.</returns>
        public static Parameter GetAssociatedParameter(Element element, BuiltInParameter connectorParameter)
        {
            var connectors = ConnectorUtils.GetConnectors(element);

            var connectorInfo = (MEPFamilyConnectorInfo)connectors.First().GetMEPConnectorInfo();

            var associatedFamilyParameterId = connectorInfo.GetAssociateFamilyParameterId(new ElementId(connectorParameter));

            if (associatedFamilyParameterId == ElementId.InvalidElementId)
                return null;

            var document = element.Document;

            var parameterElement = document.GetElement(associatedFamilyParameterId) as ParameterElement;

            if (parameterElement == null)
                return null;

            var paramterDefinition = parameterElement.GetDefinition();

            return element.get_Parameter(paramterDefinition);
        }

        /// <summary>
        /// Check if element is node element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Return true if element is of tee, spud or cross type.</returns>
        public static bool IsNodeElement(Element element)
        {
            if (element is not FamilyInstance familyInstance)
            {
                return false;
            }

            var partType = ElementUtils.GetPartType(familyInstance);
            switch (partType)
            {
                case PartType.Tee:
                    return true;
                case PartType.Cross:
                    return true;
                case PartType.TapPerpendicular:
                    return true;
                case PartType.TapAdjustable:
                    return true;
                case PartType.SpudAdjustable:
                    return true;
                case PartType.SpudPerpendicular:
                    return true;
                default:
                    return false;
            }
        }


        /// <summary>
        /// Get element's size parameters.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Returns all not nullable connector size parameters.</returns>
        public static Dictionary<Parameter, double> GetSizeParameters(Element element)
        {
            var dict = new Dictionary<Parameter, double>();

            AddParameter(GetAssociatedParameter(element, BuiltInParameter.CONNECTOR_DIAMETER), dict);
            AddParameter(GetAssociatedParameter(element, BuiltInParameter.CONNECTOR_RADIUS), dict);
            AddParameter(GetAssociatedParameter(element, BuiltInParameter.CONNECTOR_HEIGHT), dict);
            AddParameter(GetAssociatedParameter(element, BuiltInParameter.CONNECTOR_WIDTH), dict);

            return dict;
        }
        private static void AddParameter(Parameter parameter, Dictionary<Parameter, double> dictionary)
        {
            if (parameter is not null)
            {
                dictionary.Add(parameter, parameter.AsDouble());
            }
        }

        /// <summary>
        /// Disconnect element from all connected connectors.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="transactionCommit">Check if needed commit transaction</param>
        public static void Disconnect(Element element, bool transactionCommit = false)
        {
            var cons = ConnectorUtils.GetConnectors(element);
            foreach (var con1 in cons)
            {
                var connectors = con1.AllRefs;
                foreach (Connector con2 in connectors)
                {
                    if (con1.IsConnectedTo(con2))
                    {
                        if (transactionCommit)
                        {
                            ConnectorUtils.DisconnectConnectors(con1, con2);
                        }
                        else
                        {
                            con1.DisconnectFrom(con2);
                        }

                    }
                }
            }
        }
    }
}
