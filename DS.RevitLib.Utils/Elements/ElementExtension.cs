using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Points.XYZAlgorithms.MaxDistance;
using DS.RevitLib.Utils.Points.XYZAlgorithms.MaxDistance.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Extensions
{
    public static class ElementExtension
    {
        /// <summary>
        /// Check if object is null or valid
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Return false if object is null or not valid. Return true if object isn't null and valid</returns>
        public static bool NotNullValidObject(this Element element)
        {
            if (element is null || !element.IsValidObject)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get ElementType object.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Return ElementType object.</returns>
        public static ElementType GetElementType2(this Element element)
        {
            ElementId id = element.GetTypeId();
            return element.Document.GetElement(id) as ElementType;
        }

        /// <summary>
        /// Order elements list by base point.
        /// </summary>
        /// <param name="basePoint"></param>
        /// <returns>Return ordered elements by descending distances from location points to base point.</returns>
        public static List<Element> OrderByPoint(this List<Element> elements, XYZ basePoint)
        {
            var distances = new Dictionary<double, Element>();

            foreach (var elem in elements)
            {
                XYZ point = ElementUtils.GetLocationPoint(elem);
                double distance = basePoint.DistanceTo(point);
                distances.Add(distance, elem);
            }

            distances = distances.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            return distances.Values.ToList();
        }

        /// <summary>
        /// Order elements list.
        /// </summary>
        /// <param name="basePoint"></param>
        /// <returns>Return ordered elements by its locations.</returns>
        public static List<Element> Order(this List<Element> elements)
        {
            //get location points of elements
            List<XYZ> pointsList = new List<XYZ>();

            foreach (var elem in elements)
            {
                var lp = ElementUtils.GetLocationPoint(elem);
                pointsList.Add(lp);
            }

            //find edge location points
            var (point1, point2) = XYZUtils.GetMaxDistancePoints(pointsList, out double maxDist);

            var distances = new Dictionary<double, Element>();

            foreach (var elem in elements)
            {
                XYZ point = ElementUtils.GetLocationPoint(elem);
                double distance = point1.DistanceTo(point);
                distances.Add(distance, elem);
            }

            distances = distances.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            return distances.Values.ToList();
        }

        /// <summary>
        /// Get elements without spuds
        /// </summary>
        /// <param name="elements"></param>
        /// <returns>Return list of elements without spuds.</returns>
        public static List<Element> ExludeSpudes(this List<Element> elements)
        {
            var roots = new List<Element>();

            foreach (var elem in elements)
            {
                if (!elem.IsSpud())
                {
                    roots.Add(elem);
                }
            }

            return roots.Any() ? roots : elements;
        }

        public static bool IsSpud(this Element element)
        {
            if (element is not FamilyInstance familyInstance)
            {
                return false;
            }

            var pt = ElementUtils.GetPartType(familyInstance);
            if (pt == PartType.SpudPerpendicular || pt == PartType.SpudAdjustable)
            {
                return true;
            }

            return false;
        }

        public static FamilySymbol GetFamilySymbol(this Element element)
        {
            ElementId id = element.GetTypeId();
            return element.Document.GetElement(id) as FamilySymbol;
        }

        /// <summary>
        /// Get center line of element.
        /// </summary>
        /// <param name="famInst"></param>
        /// <returns>Returns center line of MEPCurve or family instance created by it's main connecors. 
        /// Returns null if element is not a MEPCuve or FamilyInstance type.</returns>
        public static Line GetCenterLine(this Element element)
        {
            if (element is MEPCurve)
            {
                return MEPCurveUtils.GetLine(element as MEPCurve);
            }
            else if (element is FamilyInstance)
            {
                FamilyInstance familyInstance = element as FamilyInstance;
                var (famInstCon1, famInstCon2) = ConnectorUtils.GetMainConnectors(familyInstance);
                return Line.CreateBound(famInstCon1.Origin, famInstCon2.Origin);
            }

            return null;
        }

        public static bool IsGeometryElement(this Element element)
        {
            var g = element.get_Geometry(new Options() { ComputeReferences = false, 
                DetailLevel = ViewDetailLevel.Fine, IncludeNonVisibleObjects = false })
                ?.Cast<GeometryObject>().ToList();

            return CheckGeometry(g);

            bool CheckGeometry(List<GeometryObject> g)
            {
                if (g is null) return false;

                foreach (var elem in g)
                {
                    if (elem is Solid s && s.Volume > 1e-6)
                        return true;
                    else if (elem is GeometryInstance gi)
                    {
                        var go = gi.GetInstanceGeometry();
                        return CheckGeometry(go?.Cast<GeometryObject>().ToList());
                    }
                }
                return false;
            }

        }

        /// <summary>
        /// Get center point of element from center of it's solid. If element is elbow returns it's location point.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Returns element center point.</returns>
        public static XYZ GetCenterPoint(this Element element)
        {
            if (!element.IsGeometryElement())
            {
                return null;
            }

            //Check elbow type
            if (element is FamilyInstance familyInstance)
            {
                var partType = ElementUtils.GetPartType(familyInstance);
                if (partType == PartType.Elbow)
                {
                    var positionPoint = element.Location as LocationPoint;
                    return positionPoint.Point;
                }
            }


            Solid solid = ElementUtils.GetSolid(element);
            return solid.ComputeCentroid();
        }
    }
}
