using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Solids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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

        public static Solid GetSolid(Element element)
        {
            var solids = SolidExtractor.GetSolids(element);
            return Solids.SolidUtils.UniteSolids(solids);
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

        /// <summary>
        /// Get the transformed solids from the coordinate space of the box to the model coordinate space.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        public static List<Solid> GetTransformedSolids(Element element, XYZ moveVector)
        {
            return SolidExtractor.GetSolids(element, moveVector);
        }

        /// <summary>
        /// Get the transformed united solid from the coordinate space of the box to the model coordinate space.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        public static Solid GetTransformedSolid(Element element, XYZ moveVector)
        {
            var solids = SolidExtractor.GetSolids(element, moveVector);
            return Solids.SolidUtils.UniteSolids(solids);
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

        public static bool DisconnectElements(Element element, Element elementToDisconnect)
        {
            List<Connector> connectors = ConnectorUtils.GetConnectors(element);

            foreach (var con in connectors)
            {
                ConnectorSet conSet = con.AllRefs;
                foreach (Connector refCon in conSet)
                {
                    if (refCon.Owner.Id == elementToDisconnect.Id && con.IsConnectedTo(refCon))
                    {
                        if (ConnectorUtils.DisconnectConnectors(con, refCon))
                        {
                            return true;
                        }
                        return false; ;
                    }
                }
            }

            return false;
        }


        public static List<FaceArray> GetFaces(Element element)
        {
            var facesArray = new List<FaceArray>();

            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement geomElem = element.get_Geometry(options);

            if (geomElem == null)
                return null;

            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)
                {
                    Solid solid = (Solid)geomObj;
                    if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                    {
                        facesArray.Add(solid.Faces);
                    }
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstance geomInst = (GeometryInstance)geomObj;
                    GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                    foreach (GeometryObject instGeomObj in instGeomElem)
                    {
                        if (instGeomObj is Solid)
                        {
                            Solid solid = (Solid)instGeomObj;
                            if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                            {
                                facesArray.Add(solid.Faces);
                            }
                        }
                    }
                }
            }

            return facesArray;
        }

        /// <summary>
        /// Get element's directions.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Return vectors(directions) between all connectors of element</returns>
        public static List<XYZ> GetDirections(Element element)
        {
            var dirs = new List<XYZ>();

            var connectors = new Stack<Connector>(ConnectorUtils.GetConnectors(element));

            while (connectors.Count > 1)
            {
                var currentConnector = connectors.Pop();
                var restConnectors = new List<Connector>(connectors);

                foreach (var con in restConnectors)
                {
                    var dir = (currentConnector.Origin - con.Origin).RoundVector().Normalize();
                    dirs.Add(dir);
                }
            }

            return dirs;
        }

        /// <summary>
        /// Get element's directions.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Return directions by main connectors of element</returns>
        public static XYZ GetMainDirection(Element element)
        {
            var (con1, con2) = ConnectorUtils.GetMainConnectors(element);
            return (con1.Origin - con2.Origin).Normalize().RoundVector();
        }

        /// <summary>
        /// Highlight elements in revit.
        /// </summary>
        /// <param name="elements"></param>
        public static void Highlight(List<Element> elements)
        {
            ICollection<ElementId> ids = new List<ElementId>();
            foreach (var elem in elements)
            {
                ids.Add(elem.Id);
            }

            UIDocument uiDoc = new UIDocument(elements.First().Document);
            uiDoc.Selection.SetElementIds(ids);
            uiDoc.ShowElements(ids);
        }

        /// <summary>
        /// Get norm vectors of element from it's faces.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Returns norm vectors of element.</returns>
        public static List<XYZ> GetOrhts(Element element)
        {
            var vectors = new List<XYZ>();
            var faces = GetFaces(element);

            foreach (var faceArray in faces)
            {
                foreach (Face face in faceArray)
                {
                    XYZ vector = face.ComputeNormal(UV.Zero);
                    vectors.Add(vector);
                }
            }

            return vectors;
        }

        /// <summary>
        /// Get norm otho vectors of element from it's faces in perpendicular plane to element's direction.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Returns norm ortho vectors of element.</returns>
        public static List<XYZ> GetOrthoNormVectors(Element element)
        {
            XYZ dir = null;
            if (element is MEPCurve)
            {
                dir = MEPCurveUtils.GetDirection(element as MEPCurve);
            }
            else if (element is FamilyInstance)
            {
                dir = element.GetCenterLine().Direction;
            }

            var orthoVectors = new List<XYZ>();
            var vectors = GetOrhts(element);

            foreach (var vector in vectors)
            {
                if (!XYZUtils.Collinearity(vector, dir))
                {
                    orthoVectors.Add(vector);
                }
            }

            return orthoVectors;
        }

        /// <summary>
        /// Get element's size by vector of element's center point.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="normVector"></param>
        /// <returns>Return distance between element's center point and intersection point between vector and element's solid.</returns>
        public static double GetSizeByVector(Element element, XYZ normVector)
        {
            List<Solid> elemSolids = GetSolids(element);
            Solid elemSolid = elemSolids.First();

            XYZ centerPoint = GetLocationPoint(element);
            Line centerLine = element.GetCenterLine();

            Line intersectLine = Line.CreateBound(centerPoint, centerPoint + normVector.Multiply(100));

            var intersectOptions = new SolidCurveIntersectionOptions();
            SolidCurveIntersection intersection = elemSolid.IntersectWithCurve(intersectLine, intersectOptions);

            XYZ intersectionPoint = null;
            if (intersection.SegmentCount != 0)
            {
                XYZ p1 = intersection.GetCurveSegment(0).GetEndPoint(0);
                XYZ p2 = intersection.GetCurveSegment(0).GetEndPoint(1);

                (XYZ minPoint, XYZ maxPoint) = XYZUtils.GetMinMaxPoints(new List<XYZ> { p1, p2 }, centerLine);
                intersectionPoint = maxPoint;
            }

            return centerLine.Distance(intersectionPoint);
        }


        /// <summary>
        /// Get orth from orths vectors by which element has maximum size.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="orths"></param>
        /// <returns></returns>
        public static XYZ GetMaxSizeOrth(Element element, List<XYZ> orths)
        {
            XYZ maxVector = null;
            double maxSize = 0;
            foreach (var vector in orths)
            {
                double size = GetSizeByVector(element, vector);
                if (size > maxSize)
                {
                    maxSize = size;
                    maxVector = vector;
                }

            }

            return maxVector;
        }

        /// <summary>
        /// Copy parameters and insulation from <paramref name="sourceElement"/> to <paramref name="targetElement"/>.
        /// </summary>
        /// <param name="sourceElement"></param>
        /// <param name="targetElement"></param>
        /// <param name="copyParameterOption"></param>
        public static void CopyConnectorParameters(FamilyInstance sourceElement, FamilyInstance targetElement, CopyParameterOption copyParameterOption)
        {
            Insulation.Create(sourceElement, targetElement);
            switch (copyParameterOption)
            {
                case CopyParameterOption.All:
                    ElementParameter.CopyAllParameters(sourceElement, targetElement);
                    break;
                case CopyParameterOption.Sizes:
                    ElementParameter.CopySizeParameters(sourceElement, targetElement);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Get total <see cref="BoundingBoxXYZ"/> by all <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns>Returns <see cref="BoundingBoxXYZ"/> with minPoint and maxPoint by min and max point of all boundingBoxes of elements. </returns>
        public static BoundingBoxXYZ GetBoundingBox(List<Element> elements)
        {

            var points = new List<XYZ>();
            foreach (var element in elements)
            {
                var bb = element.get_BoundingBox(null);
                points.Add(bb.Min);
                points.Add(bb.Max);
            }

            (XYZ minPoint, XYZ maxPoint) = XYZUtils.CreateMinMaxPoints(points);

            return new BoundingBoxXYZ() { Min = minPoint, Max = maxPoint };
        }

        /// <summary>
        /// Get total <see cref="BoundingBoxXYZ"/> by all <paramref name="points"/>.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="offset">Offset from each point from <paramref name="points"/>.</param>
        /// <returns>Returns <see cref="BoundingBoxXYZ"/> with minPoint and maxPoint by min and max values from 
        /// <paramref name="points"/> with <paramref name="offset"/>.</returns>
        public static BoundingBoxXYZ GetBoundingBox(List<XYZ> points, double offset = 0)
        {
            //Get offset points
            var lines = LineUtils.GetLines(points);
            return GetBoundingBox(lines, offset);
        }

        /// <summary>
        /// Get total <see cref="BoundingBoxXYZ"/> by all <paramref name="lines"/>.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="offset">Offset from each point from <paramref name="lines"/>.</param>
        /// <returns>Returns <see cref="BoundingBoxXYZ"/> with minPoint and maxPoint by min and max values from 
        /// <paramref name="lines"/> with <paramref name="offset"/>.</returns>
        public static BoundingBoxXYZ GetBoundingBox(List<Line> lines, double offset = 0)
        {
            //Get offset points
            var offsetPoints = new List<XYZ>();
            foreach (var line in lines)
            {
                XYZ dir = line.Direction;
                XYZ p1 = line.GetEndPoint(0);
                XYZ p2 = line.GetEndPoint(1);
                Arc circle1 = GeometryElementsUtils.CreateCircle(p1, dir, offset);
                Arc circle2 = GeometryElementsUtils.CreateCircle(p2, dir, offset);
                var offsetPoints1 = circle1.Tessellate();
                var offsetPoints2 = circle2.Tessellate();
                offsetPoints.AddRange(offsetPoints1);
                offsetPoints.AddRange(offsetPoints2);
            }

            (XYZ minPoint, XYZ maxPoint) = XYZUtils.CreateMinMaxPoints(offsetPoints);
            return new BoundingBoxXYZ() { Min = minPoint, Max = maxPoint };
        }
    }
}
