using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
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

            List<Element> elements = ConnectorUtils.GetConnectedElements(familyInstance);

            if (elements.Count !=2)
            {
                TaskDialog.Show("GetElbowCenterPoint", 
                    $"Error occured! Current connected elements count is {elements.Count }.\n" +
                    $"It must be 2 elements connected.");
                return null;
            }

            return MEPCurveUtils.GetIntersection(elements.FirstOrDefault() as MEPCurve, elements.Last() as MEPCurve);
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
        public static bool CheckMEPElement(Element element)
        {
            Type type = element.GetType();

            if (type.Name.Contains("System") | type.Name.Contains("Insulation"))
            {
                return false;
            }

            return true;
        }
    }
}
