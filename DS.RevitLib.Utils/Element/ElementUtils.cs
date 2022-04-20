using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using DS.RevitLib.Utils.Solids;

namespace DS.RevitLib.Utils
{
    public class ElementUtils
    {
        /// <summary>
        /// Get center point of any types of elements
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static XYZ GetLocationPoint(Element element)
        {
            // Get the Location property and judge whether it exists
            Location position = element.Location;

            // If the location is a point location, give the user information
            LocationPoint positionPoint = position as LocationPoint;
            if (null != positionPoint)
            {
                return positionPoint.Point;
            }
            else
            {
                // If the location is a curve location, give the user information
                LocationCurve positionCurve = position as LocationCurve;
                if (null != positionCurve)
                {
                    XYZ startPoint = positionCurve.Curve.GetEndPoint(0);
                    XYZ endPoint = positionCurve.Curve.GetEndPoint(1);

                    XYZ centerPoint = new XYZ((startPoint.X + endPoint.X) / 2,
                        (startPoint.Y + endPoint.Y) / 2,
                        (startPoint.Z + endPoint.Z) / 2);
                    return centerPoint;
                }
            }

            return null;
        }


        /// <summary>
        /// Get center point of any types of elements in millimeters
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static void GetPoints(Element element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint)
        {
            //get the current location           
            LocationCurve lc = element.Location as LocationCurve;
            Curve c = lc.Curve;
            c.GetEndPoint(0);
            c.GetEndPoint(1);

            startPoint = c.GetEndPoint(0);
            endPoint = c.GetEndPoint(1);
            centerPoint = new XYZ((startPoint.X + endPoint.X) / 2,
                (startPoint.Y + endPoint.Y) / 2,
                (startPoint.Z + endPoint.Z) / 2);

        }

        /// <summary>
        /// Get center point location of element in millimeters.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static XYZ GetLocationPointInMM(Element element)
        {
            XYZ centerPoint = GetLocationPoint(element);

            double X = UnitUtils.Convert(centerPoint.X,
                                           DisplayUnitType.DUT_DECIMAL_FEET,
                                           DisplayUnitType.DUT_MILLIMETERS);
            double Y = UnitUtils.Convert(centerPoint.Y,
                                          DisplayUnitType.DUT_DECIMAL_FEET,
                                           DisplayUnitType.DUT_MILLIMETERS);
            double Z = UnitUtils.Convert(centerPoint.Z,
                                            DisplayUnitType.DUT_DECIMAL_FEET,
                                           DisplayUnitType.DUT_MILLIMETERS);
            int X_MM = (int)Math.Round(X);
            int Y_MM = (int)Math.Round(Y);
            int Z_MM = (int)Math.Round(Z);

            XYZ point = new XYZ(X_MM, Y_MM, Z_MM);

            return point;
        }

        public static List<Solid> GetSolids(Element element)
        {
            return SolidExtractor.GetSolids(element);
        }

        public static List<Solid> GetSolidsOfElements(List<Element> elements)
        {
            List<Solid> solids = new List<Solid>();

            foreach (Element element in elements)
            {
                List<Solid> elementSolids = GetSolids(element);
                solids.AddRange(elementSolids);
            }

            return solids;
        }

        public static List<Solid> GetTransformedSolids(Element element, XYZ moveVector)
        {
            return SolidExtractor.GetSolids(element, moveVector);
        }

        public static List<Solid> GetTransformSolidsOfElements(List<Element> elements, XYZ moveVector)
        {
            List<Solid> solids = new List<Solid>();

            foreach (Element element in elements)
            {
                List<Solid> elementSolids = GetTransformedSolids(element, moveVector);
                solids.AddRange(elementSolids);
            }

            return solids;
        }

        /// <summary>
        /// Check if element subtype is MEPCurve
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsElementMEPCurve(Element element)
        {
            if (element.GetType().Name == "Pipe"
                  || element.GetType().Name == "Duct"
                  || element.GetType().Name == "Cable")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if checkingCategory coincidence with list of BuiltInCategories
        /// </summary>
        /// <param name="checkingCategory"></param>
        /// <param name="coincidenceCategories"></param>
        /// <returns></returns>
        public static bool CheckCategory(BuiltInCategory checkingCategory, List<BuiltInCategory> coincidenceCategories)
        {
            foreach (var item in coincidenceCategories)
            {
                if (checkingCategory.Equals(item))
                {
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// Select element from the list whose location point is closest to base element point;
        /// </summary>
        /// <param name="baseElement"></param>
        /// <param name="elements"></param>
        /// <returns>Return closest element. If one of the elements is not valid return another one valid by default.</returns>
        public static Element SelectClosestToElement(Element baseElement, List<Element> elements)
        {
            XYZ basePoint = GetLocationPoint(baseElement);

            return SelectClosestToPoint(basePoint, elements);
        }

        /// <summary>
        /// Select element from the list whose location point is closest to base point;
        /// </summary>
        /// <param name="baseElement"></param>
        /// <param name="elements"></param>
        /// <returns>Return closest element. If one of the elements is not valid return another one valid by default.</returns>
        public static Element SelectClosestToPoint(XYZ basePoint, List<Element> elements)
        {
            Element closestElement = elements.FirstOrDefault();

            foreach (var item in elements)
            {
                if (!item.IsValidObject)
                {
                    return elements.Where(x => x.IsValidObject).FirstOrDefault();
                }
            }

            XYZ point = GetLocationPoint(closestElement);

            double distance = basePoint.DistanceTo(point);

            if (elements.Count > 1)
            {
                for (int i = 1; i < elements.Count; i++)
                {
                    point = GetLocationPoint(elements[i]);

                    double curDistance = basePoint.DistanceTo(point);
                    if (curDistance < distance)
                    {
                        distance = curDistance;
                        closestElement = elements[i];
                    }
                }
            }

            return closestElement;
        }

        /// <summary>
        /// Get part type of family instance;
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <returns>Return part type of family instance</returns>
        public static PartType GetPartType(FamilyInstance familyInstance)
        {
            Parameter partTypeParam = familyInstance.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE);
            return (PartType)partTypeParam.AsInteger();
        }

        public static void DeleteElement(Document Doc, Element element)
        {
            using (Transaction transNew = new Transaction(Doc, "DeleteElement"))
            {
                try
                {
                    transNew.Start();
                    Doc.Delete(element.Id);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

        }


    }
}
