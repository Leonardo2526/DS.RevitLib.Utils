using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
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
        /// <param name="onlySuperd"></param>
        /// <returns>Returns parents and child connected to current familyInstance.</returns>
        public static (List<Element> parents, Element child) GetConnectedElements(this FamilyInstance familyInstance, bool onlySuperd = false)
        {
            var connectedElements = familyInstance.GetBestConnected(onlySuperd);

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


        /// <summary>
        /// Specifies if <paramref name="famInst"/> is elbow.
        /// </summary>
        /// <param name="famInst"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="famInst"/> is elbow.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool IsElbow(this FamilyInstance famInst)
        {
            PartType partType = ElementUtils.GetPartType(famInst);
            switch (partType)
            {
                case PartType.Elbow:
                    { return true; }
                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Specifies if <paramref name="famInst"/> is tee.
        /// </summary>
        /// <param name="famInst"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="famInst"/> is tee.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool IsTee(this FamilyInstance famInst)
        {
            PartType partType = ElementUtils.GetPartType(famInst);
            switch (partType)
            {
                case PartType.Tee:
                    { return true; }
                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Specifies if <paramref name="famInst"/> is spud.
        /// </summary>
        /// <param name="famInst"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="famInst"/> is spud.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool IsSpud(this FamilyInstance famInst)
        {
            PartType partType = ElementUtils.GetPartType(famInst);
            switch (partType)
            {
                case PartType.SpudPerpendicular:
                case PartType.SpudAdjustable:
                case PartType.TapAdjustable:
                case PartType.TapPerpendicular:
                    { return true; }
                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Get <paramref name="familyInstance"/> location point that specifies middle <see cref="Autodesk.Revit.DB.Connector"/>'s point.
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns>
        /// Middle point between <paramref name="familyInstance"/> <see cref="Autodesk.Revit.DB.Connector"/>'s or
        /// intersection point between <see cref="Autodesk.Revit.DB.Connector"/>'s vetors.
        /// <para>
        /// <see langword="null"/> if <paramref name="familyInstance"/> hasn't <see cref="Autodesk.Revit.DB.Connector"/>'s.
        /// </para>
        /// <para>
        /// <see cref="Autodesk.Revit.DB.Connector"/>'s origin if <paramref name="familyInstance"/> has only one <see cref="Autodesk.Revit.DB.Connector"/>.   
        /// </para>
        /// </returns>
        public static XYZ GetLocation(this FamilyInstance familyInstance)
        {
            XYZ location = null;

            var cons = ConnectorUtils.GetConnectors(familyInstance);

            switch (cons.Count)
            {
                case 0:
                    break;
                case 1:
                    {
                        location = cons[0].Origin;
                        break;
                    }
                case 2:
                    {
                        var d1 = cons[0].CoordinateSystem.BasisZ.ToVector3d();
                        var o1 = cons[0].Origin.ToPoint3d();
                        var d2 = cons[1].CoordinateSystem.BasisZ.ToVector3d();
                        var o2 = cons[1].Origin.ToPoint3d();

                        var at = 3.DegToRad();
                        double ct = Math.Pow(0.1, 3);
                        if (d1.IsParallelTo(d2, at) == 0)
                        {
                            var line1 = new Rhino.Geometry.Line(o1, d1, 1);
                            var line2 = new Rhino.Geometry.Line(o2, d2, 2);
                            Intersection.LineLine(line1, line2, out double a, out double b, ct, false);
                            location = line1.PointAt(a).ToXYZ();
                        }
                        else
                        {
                            var line = new Rhino.Geometry.Line(o1, o2);
                            location = line.PointAtLength(line.Length / 2).ToXYZ();
                        }
                        break;
                    }
                default:
                    {
                        var (con1, con2) = familyInstance.GetMainConnectors();
                        if (con1 == null || con2 == null)
                        {
                            var points = cons.Select(c => c.Origin).ToList();
                            location = XYZUtils.GetAverage(points);
                        }
                        else
                        {
                            var o1 = con1.Origin.ToPoint3d();
                            var o2 = con2.Origin.ToPoint3d();
                            var line = new Rhino.Geometry.Line(o1, o2);
                            location = line.PointAtLength(line.Length / 2).ToXYZ();
                        }
                    }
                    break;
            }

            return location;
        }
    }
}
