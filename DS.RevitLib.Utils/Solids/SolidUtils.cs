using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Solids
{
    public static class SolidUtils
    {
        /// <summary>
        /// Unite list of solids in a single solid.
        /// </summary>
        /// <param name="solids"></param>
        /// <returns>Return united solid. Return null if solids count is 0.</returns>
        public static Solid UniteSolids(List<Solid> solids, double minVolume = 0)
        {
            if (solids.Count == 0)
            {
                return null;
            }

            double minVolumeCm = UnitUtils.ConvertToInternalUnits(minVolume, DisplayUnitType.DUT_CUBIC_CENTIMETERS);

            Solid initialSolid = solids.FirstOrDefault();
            solids.Remove(initialSolid);

            foreach (var solid in solids)
            {
                if (solid.Volume < minVolumeCm)
                {
                    continue;
                }
                try
                {
                    initialSolid = BooleanOperationsUtils.ExecuteBooleanOperation(solid, initialSolid, BooleanOperationsType.Union);
                }
                catch (Exception ex)
                {
                    //TaskDialog.Show("Error", "Failed to find unite the solids. \n" + ex.Message);
                    continue;
                }
            }

            return initialSolid;
        }

        /// <summary>
        /// Get intersection solids lists of the elements.
        /// </summary>
        /// <param name="elements1"></param>
        /// <param name="elements2"></param>
        /// <returns>Return list of intersection solids.</returns>
        public static List<Solid> GetIntersection(List<Element> elements1, List<Element> elements2, double minVolume = 0)
        {
            var intersectionSolids = new List<Solid>();

            foreach (var el1 in elements1)
            {
                List<Solid> el1Solids = SolidExtractor.GetSolids(el1);
                var s1 = UniteSolids(el1Solids);
                foreach (var el2 in elements2)
                {
                    if (el1.Id == el2.Id)
                    {
                        continue;
                    }

                    List<Solid> el2Solids = SolidExtractor.GetSolids(el2);
                    var s2 = UniteSolids(el2Solids);

                    var resultSolid = GetIntersection(s1, s2, minVolume);
                    if (resultSolid is null)
                    {
                        continue;
                    }

                    intersectionSolids.Add(resultSolid);
                }
            }
            return intersectionSolids;
        }

        /// <summary>
        /// Get solid intersection of two solids.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="minVolume"></param>
        /// <returns>Return null if no intersections have been found.</returns>
        public static Solid GetIntersection(Solid s1, Solid s2, double minVolume = 0)
        {
            double minVolumeCm = UnitUtils.ConvertToInternalUnits(minVolume, DisplayUnitType.DUT_CUBIC_CENTIMETERS);
            if (s1 is not null && s2 is not null)
            {
                try
                {
                    var resultSolid = BooleanOperationsUtils.ExecuteBooleanOperation(s1, s2, BooleanOperationsType.Intersect);
                    if (resultSolid.Volume > minVolumeCm)
                    {
                        return resultSolid;
                    }
                }
                catch (Exception ex)
                { 
                    //TaskDialog.Show("Error", "Failed to find intersection between solids. \n" + ex.Message); 
                }
            }

            return null;
        }
    }
}
