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
    }
}
