using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements
{
    public static class ElementParameter
    {
        private static List<BuiltInParameter> GetExceptions()
        {
            var parameters = new List<BuiltInParameter>
            {
                BuiltInParameter.RBS_CTC_TOP_ELEVATION,
                BuiltInParameter.RBS_OFFSET_PARAM,
                BuiltInParameter.RBS_PIPE_TOP_ELEVATION,
                BuiltInParameter.RBS_PIPE_BOTTOM_ELEVATION,
                BuiltInParameter.RBS_CTC_BOTTOM_ELEVATION,
                BuiltInParameter.RBS_DUCT_TOP_ELEVATION,
                BuiltInParameter.RBS_DUCT_BOTTOM_ELEVATION,
            };
            return parameters;
        }
        private static List<BuiltInParameter> GetSizeBuiltInParameters()
        {
            var parameters = new List<BuiltInParameter>
            {
                BuiltInParameter.CONNECTOR_DIAMETER,
                BuiltInParameter.CONNECTOR_RADIUS,
                BuiltInParameter.CONNECTOR_HEIGHT,
                BuiltInParameter.CONNECTOR_WIDTH
            };
            return parameters;
        }

        /// <summary>
        /// Copy baseElement parameters to targetElement
        /// </summary>
        /// <param name="baseElement"></param>
        /// <param name="targetElement"></param>
        public static void CopyAllParameters(Element baseElement, Element targetElement)
        {
            foreach (Parameter oldp in baseElement.Parameters)
            {
                if (oldp.Definition is InternalDefinition id && GetExceptions().Contains(id.BuiltInParameter)) continue;

                var p = targetElement.get_Parameter(oldp.Definition);

                if (p is null) continue;

                if (p.IsReadOnly) continue;

                switch (p.StorageType)
                {
                    case StorageType.None:
                        break;
                    case StorageType.Integer:
                        p.Set(oldp.AsInteger());
                        break;
                    case StorageType.Double:
                        p.Set(oldp.AsDouble());
                        break;
                    case StorageType.String:
                        p.Set(oldp.AsString());
                        break;
                    case StorageType.ElementId:
                        p.Set(oldp.AsElementId());
                        break;
                    default:
                        break;
                }

            }
        }

        /// <summary>
        /// Copy baseElement size parameters to targetElement
        /// </summary>
        /// <param name="baseElement"></param>
        /// <param name="targetElement"></param>
        public static void CopySizeParameters(FamilyInstance baseElement, FamilyInstance targetElement)
        {
            var baseParameters = MEPElementUtils.GetSizeParameters(baseElement);
            var targetParameters = MEPElementUtils.GetSizeParameters(targetElement);

            foreach (var targetParam in targetParameters)
            {
                var keyValuePair = baseParameters.Where(obj => obj.Key.Id == targetParam.Key.Id).FirstOrDefault();
                targetParam.Key.Set(keyValuePair.Value);
            }
        }
    }
}
