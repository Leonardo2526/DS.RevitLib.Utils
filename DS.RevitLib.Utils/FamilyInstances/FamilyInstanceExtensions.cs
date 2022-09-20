using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.FamilyInstances
{
    /// <summary>
    /// Extension methods for FamilyInstances
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Get MEPCurve from input list that is available for connection to family instance.
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="mEPCurves"></param>
        /// <returns></returns>
        public static MEPCurve GetMEPCuveToInsert(this FamilyInstance familyInstance, List<MEPCurve> mEPCurves)
        {
            var (con1, con2) = ConnectorUtils.GetMainConnectors(familyInstance);

            foreach (var mc in mEPCurves)
            {
                var mcCons = ConnectorUtils.GetMainConnectors(mc);
                if (con1.Origin.IsBetweenPoints(mcCons.con1.Origin, mcCons.con2.Origin) &&
                    con2.Origin.IsBetweenPoints(mcCons.con1.Origin, mcCons.con2.Origin))
                {
                    return mc;
                }
            }

            return null;
        }
    }
}
