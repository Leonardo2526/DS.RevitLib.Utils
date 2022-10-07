using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Get elements connected to current familyInstance.
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns>Returns parents and child connected to current familyInstance.</returns>
        public static (List<Element> parents, Element child) GetConnectedElements(this FamilyInstance familyInstance)
        {
            List<Element> connectedElements = ConnectorUtils.GetConnectedElements(familyInstance).ToList();

            var relationBuilder = new FamInstRelationBuilder(familyInstance);
            if (relationBuilder.Builer is null)
            {
                return (connectedElements, null);
            }

            var parents = new List<Element>();
            Element child = null;

            foreach (var mEPCurve in connectedElements)
            {
                var relation = relationBuilder.GetRelation(mEPCurve);
                if (relation == Relation.Parent)
                {
                    parents.Add(mEPCurve);
                }
                else
                {
                    child = mEPCurve;
                }
            }

            return (parents, child);
        }
    }
}
