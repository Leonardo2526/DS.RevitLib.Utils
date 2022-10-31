using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils
{
    public class PartTypesChecker
    {
        /// <summary>
        /// Check familyInstance for PartType
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns>Return true if part type is tap.</returns>
        public static bool IsSpud(FamilyInstance familyInstance)
        {
            PartType partType = ElementUtils.GetPartType(familyInstance);

            if (partType == PartType.SpudPerpendicular)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check familyInstance for PartType
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns>Return true if part type is elbow.</returns>
        public static bool IsElbow(FamilyInstance familyInstance)
        {
            PartType partType = ElementUtils.GetPartType(familyInstance);

            if (partType == PartType.Elbow)
            {
                return true;
            }
            return false;

        }

        /// <summary>
        /// Check familyInstance for PartType
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns>Return true if part type is tee.</returns>
        public static bool IsTee(FamilyInstance familyInstance)
        {
            PartType partType = ElementUtils.GetPartType(familyInstance);

            if (partType == PartType.Tee)
            {
                return true;
            }
            return false;

        }
    }
}
