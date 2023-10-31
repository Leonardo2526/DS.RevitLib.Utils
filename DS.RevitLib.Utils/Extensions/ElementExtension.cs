using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Connection;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Transactions;
using DS.RevitLib.Utils.Visualisators;
using System.Collections.Generic;
using System.Linq;
using Vector3d = Rhino.Geometry.Vector3d;

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
        /// <param name="elements"></param>
        /// <returns>Return ordered elements by descending distances from location points to base point.</returns>       
        public static List<Element> OrderByPoint(this List<Element> elements, XYZ basePoint)
        {
            var distances = new Dictionary<Element, double>();

            foreach (var elem in elements)
            {
                XYZ point = ElementUtils.GetLocationPoint(elem);
                double distance = basePoint.DistanceTo(point);
                distances.Add(elem, distance);
            }

            distances = distances.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            return distances.Keys.ToList();
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

        /// <summary>
        /// Check if element is spud or tap.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsSpud(this Element element)
        {
            if (element is not FamilyInstance familyInstance)
            {
                return false;
            }

            var pt = ElementUtils.GetPartType(familyInstance);
            if (pt == PartType.SpudPerpendicular || pt == PartType.SpudAdjustable ||
                pt == PartType.TapPerpendicular || pt == PartType.TapAdjustable)
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
        /// <param name="element"></param>
        /// <returns>
        /// <see cref="LocationCurve"/> as <see cref="Line"/> of <paramref name="element"/>.
        /// <para>
        /// <see cref="Line"/> between main connectors if <paramref name="element"/> is <see cref="Autodesk.Revit.DB.FamilyInstance"/>.
        /// </para>
        /// <para>
        /// <see langword="null"/> if it was failed to get <see cref="Line"/> from <paramref name="element"/> .
        /// </para>
        /// </returns>
        public static Line GetCenterLine(this Element element)
        {
            if (element is FamilyInstance)
            {
                FamilyInstance familyInstance = element as FamilyInstance;
                var (famInstCon1, famInstCon2) = ConnectorUtils.GetMainConnectors(familyInstance);
                if (famInstCon1 == null || famInstCon2 == null
                    || famInstCon1.Origin.DistanceTo(famInstCon2.Origin) < 0.001) { return null; }
                return Line.CreateBound(famInstCon1.Origin, famInstCon2.Origin);
            }
            else
            {
                var lCurve = element.Location as LocationCurve;
                return lCurve == null ? null : lCurve.Curve as Line;
            }
        }

        /// <summary>
        /// Check if element is geomety element.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="includeLines"></param>
        /// <returns>
        /// <see langword="true"/> if element is <see cref="GeometryObject"/> and has volume of solid.
        /// <para>
        /// <see langword="true"/> if <paramref name="includeLines"/> parameter is <see langword="true"/> and element is <see cref="Line"/>.     
        /// </para>
        /// <para>
        /// Otherwise returns <see langword="false"/>; 
        /// </para>
        /// </returns>
        public static bool IsGeometryElement(this Element element, bool includeLines = false)
        {
            var g = element.get_Geometry(new Options()
            {
                ComputeReferences = false,
                DetailLevel = ViewDetailLevel.Fine,
                IncludeNonVisibleObjects = false
            })
                ?.Cast<GeometryObject>().ToList();

            return CheckGeometry(g);

            bool CheckGeometry(List<GeometryObject> g)
            {
                if (g is null) return false;

                foreach (var elem in g)
                {
                    if (elem is Solid s && s.Volume > 1e-6)
                    { return true; }
                    else if (includeLines && elem is Line) { return true; }
                    else if (elem is GeometryInstance gi)
                    {
                        var go = gi.GetInstanceGeometry();
                        if (CheckGeometry(go?.Cast<GeometryObject>().ToList())) { return true; };
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

        /// <summary>
        /// Get center point of any types of elements without concider main connectors and its directions.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>
        /// <paramref name="element"/>'s location point based on it's <see cref="Location"/>.
        /// </returns>
        public static XYZ GetLocationPoint(this Element element)
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
        /// Show all edges of elements solid.
        /// </summary>
        /// <param name="element"></param>
        /// <remarks>Transaction is not provided, so methods should be wrapped to transacion.</remarks>
        public static void ShowEdges(this Element element)
        {
            Document doc = element.Document;
            var solids = ElementUtils.GetSolids(element);
            foreach (Solid s in solids)
            {
                s.ShowEdges(doc);
            }
        }

        /// <summary>
        /// Show <see cref="BoundingBoxXYZ"/> of <paramref name="element"/> in model.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="transactionBuilder"></param>
        public static void ShowBoundingBox(this Element element, AbstractTransactionBuilder transactionBuilder = null)
        {
            var doc = element.Document;
            transactionBuilder ??= new TransactionBuilder(doc);

            BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
            transactionBuilder.Build(() =>
            {
                var visualizator = new BoundingBoxVisualisator(boundingBox, doc);
                visualizator.Show();
            }, "show BoundingBox");
        }

        /// <summary>
        /// Show <see cref="BoundingBoxXYZ"/> of link <paramref name="element"/> in model.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="revitLink"></param>
        /// <param name="linkDoc">Link document containing <paramref name="element"/>.</param>
        /// <param name="transactionBuilder"></param>
        public static void ShowBoundingBox(this Element element, RevitLinkInstance revitLink,
            AbstractTransactionBuilder transactionBuilder = null)
        {
            var doc = revitLink.Document;
            transactionBuilder ??= new TransactionBuilder(doc);

            Transform transform = revitLink.GetTotalTransform();

            BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
            var transformBoundingBox = new BoundingBoxXYZ()
            {
                Min = transform.OfPoint(boundingBox.Min),
                Max = transform.OfPoint(boundingBox.Max)
            };
            transactionBuilder.Build(() =>
            {
                var visualizator = new BoundingBoxVisualisator(transformBoundingBox, doc);
                visualizator.Show();
            }, "show BoundingBox");
        }

        /// <summary>
        /// Check if current <paramref name="element"/> is conform given <paramref name="categoriesDict"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="categoriesDict"></param>
        /// <returns>Returns true if it is conform both <paramref name="categoriesDict"/> key and value or 
        /// only key if <see cref="Autodesk.Revit.DB.PartType"/> value is undefined. 
        /// <para>Returns true if <paramref name="categoriesDict"/> is null or empty.</para>
        /// <para>Returns false if element is not <see cref="Autodesk.Revit.DB.FamilyInstance"/>.</para>
        /// </returns>
        public static bool IsCategoryElement(this Element element, Dictionary<BuiltInCategory, List<PartType>> categoriesDict)
        {
            if (categoriesDict is null || !categoriesDict.Any()) { return true; }
            if (element is not FamilyInstance) { return false; }

            var builtCat = CategoryExtension.GetBuiltInCategory(element.Category);
            var partType = ElementUtils.GetPartType(element as FamilyInstance);

            var dictBuilCat = categoriesDict.Keys.FirstOrDefault(c => (int)c == (int)builtCat);
            if (dictBuilCat is not 0)
            {
                categoriesDict.TryGetValue(dictBuilCat, out var dictPartTypes);
                foreach (var item in dictPartTypes)
                {
                    if (item == PartType.Undefined || partType == item)
                    {
                        return true;
                    }
                }
            }


            return false;
        }

        /// <summary>
        /// Connect <paramref name="elements"/> by common connectors.
        /// </summary>
        /// <param name="elements"></param>
        public static void Connect(this List<Element> elements)
        {
            for (int i = 0; i < elements.Count - 1; i++)
            {
                (Connector con1, Connector con2) = ConnectorUtils.GetCommonNotConnectedConnectors(elements[i], elements[i + 1]);
                if (con1 is not null && con2 is not null && !con1.IsConnectedTo(con2)) { con1.ConnectTo(con2); };
            }
        }

        /// <summary>
        /// Connect <paramref name="element"/> to <paramref name="element1"/> and <paramref name="element2"/>.
        /// <para>
        /// Set <paramref name="element2"/> as parent in case of tee connection.
        /// </para>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        public static void Connect(this Element element, Element element1, Element element2 = null)
        {
            List<Element> elements = new List<Element>() { element, element1 };
            if (element2 != null) { elements.Add(element2); }

            Document doc = element.Document;
            MEPCurve mEPCurve = element as MEPCurve;
            MEPCurve mEPCurve1 = element1 as MEPCurve;
            MEPCurve mEPCurve2 = element2 as MEPCurve;

            if (mEPCurve is not null && mEPCurve1 is not null)
            {
                IConnectionFactory factory = new MEPCurveConnectionFactory(doc, mEPCurve, mEPCurve1, mEPCurve2);
                factory.Connect();
            }
            else
            {
                elements.Connect();
            }
        }

        /// <summary>
        /// Get element's size by vector of element's center point or by <paramref name="innerPoint"/> if it was specified.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="normVector"></param>
        /// <param name="innerPoint"></param>
        /// <returns>Return distance between element's center point and intersection point between vector and element's solid.</returns>
        public static double GetSizeByVector(this Element element, XYZ normVector, XYZ innerPoint = null)
        {
            List<Solid> elemSolids = ElementUtils.GetSolids(element);
            Solid elemSolid = elemSolids.First();

            XYZ centerPoint = innerPoint ?? element.GetCenterPoint();
            Line centerLine = element.GetCenterLine();
            centerPoint = centerLine.Project(centerPoint).XYZPoint;

            Line intersectLine = Line.CreateBound(centerPoint, centerPoint + normVector.Multiply(100));

            var intersectOptions = new SolidCurveIntersectionOptions()
            {
                ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
            };
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
        /// Get <paramref name="element"/>'s  insulation.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>
        /// Returns <paramref name="element"/>'s <see cref="InsulationLiningBase"/> if it has it. 
        /// Otherwise returns null.</returns>
        public static InsulationLiningBase GetInsulation(this Element element)
        {
            return element is Pipe || element is Duct ?
                InsulationLiningBase.GetInsulationIds(element.Document, element.Id)
               .Select(x => element.Document.GetElement(x) as InsulationLiningBase).FirstOrDefault() :
               null;
        }

        /// <summary>
        /// Check if <paramref name="element"/> contains <paramref name="point"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="point"></param>
        /// <returns>Returns true if <paramref name="point"/> is inside <paramref name="element"/>.</returns>
        public static bool Contains(this Element element, XYZ point)
        {
            double multiplicator = 100;
            Line line1 = Line.CreateBound(point, point + XYZUtils.GenerateXYZ().Multiply(multiplicator));

            var faces = element.Solid().Faces;
            int intersectionCount = 0;
            foreach (Face face in faces)
            {
                if (face.Intersect(line1) == SetComparisonResult.Overlap)
                { intersectionCount++; }
            }

            return intersectionCount % 2 != 0;
        }

        /// <summary>
        /// Get <paramref name="element"/>'s solid.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Solid Solid(this Element element) => ElementUtils.GetSolid(element);

        /// <summary>
        /// Get <paramref name="element"/>'s <see cref="Autodesk.Revit.DB.Solid"/> if <see cref="RevitLinkInstance"/> contais it.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="document"></param>
        /// <returns>
        /// <paramref name="element"/>'s <see cref="Autodesk.Revit.DB.Solid"/> by <see cref="RevitLinkInstance"/> transform.
        /// <para>
        /// <paramref name="element"/>'s <see cref="Autodesk.Revit.DB.Solid"/> without any transform if it's <see cref="Document"/> is not a <see cref="RevitLinkInstance"/>.
        /// </para>
        /// </returns>
        public static Solid GetSolidInLink(this Element element, Document document)
        {
            return element.Document.IsLinked ?
                element.GetTransformed(element.GetLink(document)) :
                element.Solid();
        }

        /// <summary>
        /// Get <paramref name="element"/>'s solid with it's insulation.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>
        /// <paramref name="element"/>'s <see cref="Autodesk.Revit.DB.Solid"/> with it's insulation if it has it.
        /// <para>
        /// Otherwise returns only <paramref name="element"/>'s <see cref="Autodesk.Revit.DB.Solid"/>.
        /// </para>
        /// </returns>
        public static Solid GetSolidWithInsulation(this Element element)
        {
            Solid solid = element.Solid();

            List<Solid> solids = new()
            {solid};

            Element insulation = element.GetInsulation();
            if (insulation is not null && insulation.IsValidObject)
            {
                solids.Add(insulation.Solid());
                solid = Solids.SolidUtils.UniteSolids(solids);
            }

            return solid;
        }

        /// <summary>
        /// Get connected elements to <paramref name="element"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>
        /// Returns connected elements by one from each <paramref name="element"/>'s connector.
        /// <para>
        /// Returns empty list if no connected elements was found.       
        /// </para>
        /// </returns>
        public static List<Element> GetConnected(this Element element) => ConnectorUtils.GetConnectedElements(element);

        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.Element"/>'s connected to <paramref name="element"/>.
        /// <para>
        /// Get only super connected <see cref="Autodesk.Revit.DB.Element"/>'s if <paramref name="onlySuperb"/> is set to it's default <see langword="true"/> value.
        /// It's also checks <paramref name="element"/>'s subelements for connection.    
        /// </para>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="onlySuperb"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Element"/>'s list that has common <see cref="Autodesk.Revit.DB.Connector"/>'s with <paramref name="element"/>.
        /// <para>
        /// Empty list if <paramref name="element"/> hasn't connected <see cref="Autodesk.Revit.DB.Element"/>'s.
        /// </para>
        /// </returns>
        public static List<Element> GetBestConnected(this Element element, bool onlySuperb = true ) =>
            ConnectorUtils.GetBestConnectedElements(element, onlySuperb);

        /// <summary>
        /// Get main connectors of element. 
        /// </summary>
        /// <param name="element"></param>
        /// <returns>If element is MEPCurve returns two connectors of it with max distance between them.
        /// If element is FamilyInstance returns two connectors if element's location point is on line between them.
        /// <para>
        /// Otherwise (<see langword="null"/>, <see langword="null"/>).
        /// </para>
        /// </returns>
        public static (Connector con1, Connector con2) GetMainConnectors(this Element element) =>
            ConnectorUtils.GetMainConnectors(element);

        /// <summary>
        /// Converet <paramref name="elements"/> to list of only valid elements.
        /// </summary>
        /// <param name="elements"></param>
        public static void ConvertToValid(this List<Element> elements)
        {
            if (
                elements is not null &&
                elements.Any() &&
                elements.TrueForAll(obj => obj.IsValidObject))
            { return; }

            var indexesToRemove = new List<int>();
            for (int i = 0; i < elements.Count; i++)
            {
                Element obj = elements[i];
                if (!obj.IsValidObject)
                { indexesToRemove.Add(i); }
            }
            indexesToRemove.ForEach(elements.RemoveAt);
        }

        /// <summary>
        /// Specify if <paramref name="element"/> is MEP element.    
        /// </summary>
        /// <param name="element"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="element"/> is pipe, duct, cable tray, fitting, accessory or equipment.
        /// </returns>
        public static bool IsMEPElement(this Element element)
        {
            var type = element.GetType();
            bool validType = type == typeof(Pipe) || type == typeof(Duct) || type == typeof(CableTray) ? true : false;
            if (validType) { return true; }

            var familyInstance = element as FamilyInstance;
            if (familyInstance?.MEPModel.ConnectorManager is null) { return false; }

            BuiltInCategory familyInstanceCategory = CategoryExtension.GetBuiltInCategory(element.Category);

            List<BuiltInCategory> builtInCategories = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_PipeFitting,
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_CableTrayFitting,
            BuiltInCategory.OST_MechanicalEquipment,
            BuiltInCategory.OST_DuctAccessory,
            BuiltInCategory.OST_PipeAccessory};

            return ElementUtils.CheckCategory(familyInstanceCategory, builtInCategories);
        }

        /// <summary>
        /// Get <see cref="RevitLinkInstance"/> by <paramref name="element"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// <see cref="RevitLinkInstance"/> from loaded links in <paramref name="doc"/> if there is any that contains <paramref name="element"/>.
        /// <para>
        /// <see langword="null"/> if no loaded links are in <paramref name="doc"/> or no links that contains <paramref name="element"/>.
        /// </para>
        /// </returns>
        public static RevitLinkInstance GetLink(this Element element, Document doc) =>
            doc.TryGetLink(element.Document);


        /// <summary>
        /// Get transformed solid from <paramref name="element"/> by <paramref name="revitLink"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="revitLink"></param>
        /// <returns>
        /// Real <paramref name="element"/> <see cref="Autodesk.Revit.DB.Solid"/> positions in current document.
        /// </returns>
        public static Solid GetTransformed(this Element element, RevitLinkInstance revitLink)
        {
            var solid = element.Solid();
            if (solid is null)
            { return null; }

            var linkTransform = revitLink.GetTotalTransform();
            if (!linkTransform.AlmostEqual(Transform.Identity))
            {
                solid = SolidUtils.CreateTransformed(solid, linkTransform);
            }

            return solid;
        }

        /// <summary>
        /// Check if current <paramref name="element"/>'s <see cref="Autodesk.Revit.DB.BuiltInCategory"/> is conform to given <paramref name="builtInCategories"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="builtInCategories"></param>
        /// <returns>
        /// <see langword="true"/> if any from <paramref name="builtInCategories"/> contains <paramref name="element"/>'s  <see cref="Autodesk.Revit.DB.BuiltInCategory"/>.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool IsCategoryElement(this Element element, IEnumerable<BuiltInCategory> builtInCategories)
        {
            BuiltInCategory elementCategory = CategoryExtension.GetBuiltInCategory(element.Category);
            return ElementUtils.CheckCategory(elementCategory, builtInCategories);
        }

        /// <summary>
        /// Specifies if the <paramref name="wall"/> is traversable by <paramref name="traverseDirection"/>.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="traverseDirection"></param>
        /// <param name="parameterName"></param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="wall"/> has parameter with <paramref name="parameterName"/> with value as <see langword="false"/>
        /// and <paramref name="traverseDirection"/> perpendicular to <paramref name="wall"/> direction.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool IsTraversable(this Wall wall, Vector3d traverseDirection, string parameterName = "OLP_БезПересечений")
        {
            var p = wall.GetParameters(parameterName).FirstOrDefault();
            if (p == null || p.AsInteger() == 1) { return false; }

            var wDir = wall.GetCenterLine().Direction.ToVector3d();
            wDir = Vector3d.Divide(wDir, wDir.Length);
            if (traverseDirection.IsPerpendicularTo(wDir, 3.DegToRad()))
            { return true; }

            return false;
        }

        /// <summary>
        /// Specifies if the <paramref name="wall"/> is traversable by <paramref name="traverseDirection"/>.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="traverseDirection"></param>
        /// <param name="parameterName"></param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="wall"/> has parameter with <paramref name="parameterName"/> with value as <see langword="false"/>
        /// and <paramref name="traverseDirection"/> perpendicular to <paramref name="wall"/> direction.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool IsTraversable(this Wall wall, XYZ traverseDirection, string parameterName = "OLP_БезПересечений") =>
           wall.IsTraversable(traverseDirection.ToVector3d(), parameterName);

        /// <summary>
        /// Specifies if <paramref name="element"/> is connected to <paramref name="checkElement"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="checkElement"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="element"/> has common connectors with <paramref name="checkElement"/>.
        /// <para>
        /// Otherwise returns <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool IsConnected(this Element element, Element checkElement)
        {
            var (elem1Con, elem2Con) = ConnectorUtils.GetCommonConnectors(element, checkElement);
            return (elem1Con is not null && elem2Con is not null);
        }

        /// <summary>
        /// Get (<see cref="Autodesk.Revit.DB.XYZ"/> , <see cref="Autodesk.Revit.DB.XYZ"/>) 
        /// that specify <see cref="Autodesk.Revit.DB.XYZ"/>'s on floor and ceiling that are closest to <see cref="ConnectionPoint"/>.
        /// <para>
        /// If <see cref="Autodesk.Revit.DB.Element"/> of <paramref name="pointElement"/> is <see langword="null"/> the distance to floor/ceiling
        /// will be checked from <see cref="Autodesk.Revit.DB.XYZ"/> of <paramref name="pointElement"/>.
        /// </para>
        /// </summary>
        /// <returns>
        /// (<see cref="Autodesk.Revit.DB.XYZ"/> , <see cref="Autodesk.Revit.DB.XYZ"/>)
        /// if <see cref="ConnectionPoint"/> is within floor and ceiling limits.
        /// <para> 
        /// (<see langword="null"/>, <see langword="null"/>) 
        /// if <see cref="ConnectionPoint"/> is outside floor and ceiling limits.</para>
        /// </returns>
        public static (XYZ pointFloorBound, XYZ pointCeilingBound) GetFloorBounds(this (Element element, XYZ point) pointElement,
            Document doc,
            double minDistToFloor, double minDistToCeiling,
            bool isInsulationAccount = true,
            int distnaceToFindFloor = 30)
        {
            var element = pointElement.element;
            var point = pointElement.point;

            double h2 = element is null ? 0 : element.GetSizeByVector(XYZ.BasisZ, point);
            var ins = isInsulationAccount && element is not null && element is MEPCurve mEPCurve ?
                mEPCurve.GetInsulationThickness() : 0;
            var hmin = h2 + ins;

            double offsetFromFloor = hmin + minDistToFloor;
            double offsetFromCeiling = hmin + minDistToCeiling;

            var minHFloor = offsetFromFloor;
            var minHCeiling = offsetFromCeiling;

            XYZ pointFloorBound = point.GetXYZBound(doc, minHFloor, -distnaceToFindFloor);
            XYZ pointCeilingBound = point.GetXYZBound(doc, minHCeiling, distnaceToFindFloor);

            return (pointFloorBound, pointCeilingBound);
        }
    }
}
