using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP
{
    public static class MEPCurveParameter
    {
        /// <summary>
        /// Copy baseMEPCurve parameters to targetMEPCurve
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <param name="targetMEPCurve"></param>
        public static void Copy(MEPCurve baseMEPCurve, MEPCurve targetMEPCurve)
        {
            var exceptions = new List<BuiltInParameter>()
            {
                BuiltInParameter.RBS_CTC_TOP_ELEVATION,
                BuiltInParameter.RBS_OFFSET_PARAM,
                BuiltInParameter.RBS_PIPE_TOP_ELEVATION,
                BuiltInParameter.RBS_PIPE_BOTTOM_ELEVATION,
                BuiltInParameter.RBS_CTC_BOTTOM_ELEVATION,
                BuiltInParameter.RBS_DUCT_TOP_ELEVATION,
                BuiltInParameter.RBS_DUCT_BOTTOM_ELEVATION,
            };
           
            foreach (Parameter oldp in baseMEPCurve.Parameters)
            {
                if (oldp.Definition is InternalDefinition id && exceptions.Contains(id.BuiltInParameter)) continue;

                var p = targetMEPCurve.get_Parameter(oldp.Definition);

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


    }
}
