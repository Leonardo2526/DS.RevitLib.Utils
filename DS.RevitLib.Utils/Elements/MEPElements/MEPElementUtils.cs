﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.Models;
using Ivanov.RevitLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP
{
    public static class MEPElementUtils
    {
        public static XYZ GetElbowCenterPoint(FamilyInstance familyInstance)
        {
            BuiltInCategory familyInstanceCategory = CategoryExtension.GetBuiltInCategory(familyInstance.Category);

            List<BuiltInCategory> builtInCategories = new List<BuiltInCategory>
            { BuiltInCategory.OST_PipeFitting, BuiltInCategory.OST_DuctFitting, BuiltInCategory.OST_CableTrayFitting};

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

            if (type.Name.Contains("System") | type.Name.Contains("Insulation") | type.Name.Contains("Connector"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get element's parameter associated with parameter of connectors.
        /// </summary>
        /// <param name="famInst"></param>
        /// <param name="connectorParameter"></param>
        /// <returns>Return assiciated parameter.</returns>
        public static Parameter GetAssociatedParameter(FamilyInstance famInst, BuiltInParameter connectorParameter)
        {
            var connectors = ConnectorUtils.GetConnectors(famInst);

            var connectorInfo = (MEPFamilyConnectorInfo)connectors.First().GetMEPConnectorInfo();

            var associatedFamilyParameterId = connectorInfo.GetAssociateFamilyParameterId(new ElementId(connectorParameter));

            if (associatedFamilyParameterId == ElementId.InvalidElementId)
                return null;

            var document = famInst.Document;

            var parameterElement = document.GetElement(associatedFamilyParameterId) as ParameterElement;

            if (parameterElement == null)
                return null;

            var paramterDefinition = parameterElement.GetDefinition();

            return famInst.get_Parameter(paramterDefinition);
        }

        /// <summary>
        /// Get <paramref name="famInst"/>'s <paramref name="connector"/> parameter associated with parameter connectors.
        /// </summary>
        /// <param name="famInst"></param>
        /// <param name="connector"></param>
        /// <param name="connectorParameter"></param>
        /// <returns>Return assiciated parameter.</returns>
        public static Parameter GetAssociatedParameter(FamilyInstance famInst, Connector connector, BuiltInParameter connectorParameter)
        {          
            var connectorInfo = (MEPFamilyConnectorInfo)connector.GetMEPConnectorInfo();

            var associatedFamilyParameterId = connectorInfo.GetAssociateFamilyParameterId(new ElementId(connectorParameter));

            if (associatedFamilyParameterId == ElementId.InvalidElementId)
                return null;

            var document = famInst.Document;

            var parameterElement = document.GetElement(associatedFamilyParameterId) as ParameterElement;

            if (parameterElement == null)
                return null;

            var paramterDefinition = parameterElement.GetDefinition();

            return famInst.get_Parameter(paramterDefinition);
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
        /// <param name="famInst"></param>
        /// <returns>Returns all not nullable connector size parameters.</returns>
        public static Dictionary<Parameter, double> GetSizeParameters(FamilyInstance famInst)
        {
            var dict = new Dictionary<Parameter, double>();

            AddParameter(GetAssociatedParameter(famInst, BuiltInParameter.CONNECTOR_DIAMETER), dict);
            AddParameter(GetAssociatedParameter(famInst, BuiltInParameter.CONNECTOR_RADIUS), dict);
            AddParameter(GetAssociatedParameter(famInst, BuiltInParameter.CONNECTOR_HEIGHT), dict);
            AddParameter(GetAssociatedParameter(famInst, BuiltInParameter.CONNECTOR_WIDTH), dict);

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

        /// <summary>
        /// Connect <paramref name="element1"/> with <paramref name="element2"/> if they have common connector.
        /// </summary>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        public static void Connect(Element element1, Element element2)
        {          
            var (elem1Con, elem2Con) = ConnectorUtils.GetCommonNotConnectedConnectors(element1, element2);
            if(elem1Con is null || elem2Con is null) { return; }
            if(elem1Con.IsConnectedTo(elem2Con)) { return; }
            else
            {elem1Con.ConnectTo(elem2Con);}
        }

        /// <summary>
        /// Get <see cref="MEPSystemType"/> by type of <paramref name="baseMEPCurve"/>.
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <returns>Returns first <see cref="MEPSystemType"/> that match to type of <paramref name="baseMEPCurve"/>.</returns>
        public static MEPSystemType GetDefaultMepSystemType(MEPCurve baseMEPCurve)
        {
            string elementTypeName = baseMEPCurve.GetType().Name;
            var collector = new FilteredElementCollector(baseMEPCurve.Document).OfClass(typeof(ElementType));

            ElementClassFilter systemFilter = null;
            if(elementTypeName == "Pipe") { systemFilter = new ElementClassFilter(typeof(PipingSystemType)); }
            else if (elementTypeName == "Duct") { systemFilter = new ElementClassFilter(typeof(MechanicalSystemType)); }

            var systems = collector.WherePasses(systemFilter).ToElements().Cast<MEPSystemType>();
            return systems.FirstOrDefault();
        }

        /// <summary>
        /// Get <see cref="MEPSystem"/> of <paramref name="baseMEPCurve"/>.
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <returns>Returns first <see cref="MEPSystem"/> that match to type of <paramref name="baseMEPCurve"/></returns>
        public static MEPSystem GetDefaultMepSystem(MEPCurve baseMEPCurve)
        {
            string elementTypeName = baseMEPCurve.GetType().Name;
            var systemCollector = new FilteredElementCollector(baseMEPCurve.Document).OfClass(typeof(MEPSystem));
            IEnumerable<MEPSystem> desirableSystems = systemCollector.Cast<MEPSystem>();

            foreach (MEPSystem system in desirableSystems)
            {
                var systemType = system.GetType();
                if (elementTypeName == "Pipe" && systemType == typeof(PipingSystem))
                {
                    return system;
                }
                if (elementTypeName == "Duct" && systemType == typeof(MechanicalSystem))
                {
                    return system;
                }
            }

            return null;
        }

        /// <summary>
        /// Get elements of <see cref="MEPSystem"/> with <paramref name="strMEPSysName"/> name.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="strMEPSysName"></param>
        /// <param name="canContain">Specify whether system name should match exactly with <paramref name="strMEPSysName"/>,</param>
        /// <returns>Returns all elements in first <see cref="MEPSystem"/> with <paramref name="strMEPSysName"/>.</returns>
        public static List<Element> GetSystemElements(Document doc, string strMEPSysName,bool canContain = false) => 
            new MEPSystemElement(doc).GetSystemElements(strMEPSysName, canContain);
        
    }
}
