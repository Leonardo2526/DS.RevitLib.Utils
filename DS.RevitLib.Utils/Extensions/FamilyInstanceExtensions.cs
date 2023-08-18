using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitLib.Utils.Extensions
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

        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.PartType"/> of <see cref="Autodesk.Revit.DB.FamilyInstance"/>.
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns></returns>
        public static PartType GetPartType(this FamilyInstance familyInstance)
        {
            Parameter partTypeParam = familyInstance.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE);
            return (PartType)partTypeParam.AsInteger();
        }

        /// <summary>
        /// Gets all included the sub component ElementIds of the <paramref name="famInst"/>. 
        /// </summary>
        /// <param name="famInst"></param>
        /// <returns>
        /// Included sub component elementIds of the <paramref name="famInst"/> without <paramref name="famInst"/> itself. 
        /// <para>
        /// Empty list if <paramref name="famInst"/> doesn't contains any sub components.
        /// </para>
        /// <para>
        /// <see langword="null"/> if <paramref name="famInst"/> is not a <see cref="Autodesk.Revit.DB.FamilyInstance"/>.       
        /// </para>
        /// </returns>
        public static List<ElementId> GetSubAllElementIds(this FamilyInstance famInst)
        {
            var subElementIds = new List<ElementId>();

            if (famInst == null)
            { Debug.WriteLine("Failed to get sub elements: top family instance is null."); return null; }

            Document doc = famInst.Document;

            List<ElementId> currentElemIds = famInst.GetSubComponentIds().
                Where(comp => doc.GetElement(comp).IsGeometryElement()).
                ToList();
            if (currentElemIds.Count == 0) { return subElementIds; }

            subElementIds.AddRange(currentElemIds);
            foreach (var item in currentElemIds)
            {
                var itemFamInst = doc.GetElement(item) as FamilyInstance;
                if (itemFamInst != null) { subElementIds.AddRange(GetSubAllElementIds(itemFamInst)); }
            }

            return subElementIds;
        }
    }
}
