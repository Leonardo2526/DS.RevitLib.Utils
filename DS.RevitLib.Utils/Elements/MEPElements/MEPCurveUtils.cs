using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP
{
    public static class MEPCurveUtils
    {
        public static double GetLength(MEPCurve mEPCurve)
        {
            LocationCurve lc = mEPCurve.Location as LocationCurve;
            return lc.Curve.ApproximateLength;
        }

        /// <summary>
        /// Get intersercion between two MEPCurves. 
        /// </summary>
        /// <param name="mEPCurve1"></param>
        /// <param name="mEPCurve2"></param>
        /// <returns>Return real or virtual (in 10 feets range) intersection point. Return null if no intersection exist.</returns>
        public static XYZ GetIntersection(MEPCurve mEPCurve1, MEPCurve mEPCurve2)
        {
            Line line1 = mEPCurve1.GetCurve() as Line;
            Line line2 = mEPCurve2.GetCurve() as Line;

            line1 = line1.IncreaseLength(10);
            line2 = line2.IncreaseLength(10);

            _ = line1.GetEndPoint(0);
            _ = line1.GetEndPoint(1);
            _ = line2.GetEndPoint(0);
            _ = line2.GetEndPoint(1);


            line1.Intersect(line2, out IntersectionResultArray resultArray);

            if (resultArray is null)
            {
                return null;
            }

            return resultArray.get_Item(0).XYZPoint;
        }

        /// <summary>
        /// Get MEPCurve's direction.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Return vector(direction)</returns>
        public static XYZ GetDirection(MEPCurve mEPCurve)
        {
            var line = GetLine(mEPCurve);
            return line.Direction;
        }

        /// <summary>
        /// Get MEPCurve's line.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Return vector(direction)</returns>
        public static Line GetLine(MEPCurve mEPCurve)
        {
            var locCurve = mEPCurve.Location as LocationCurve;
            return locCurve.Curve as Line;
        }

        /// <summary>
        /// Get angle berween two MEPCurves in rads.
        /// </summary>
        /// <param name="mEPCurve1"></param>
        /// <param name="mEPCurve2"></param>
        /// <returns>Return angle berween two MEPCurves in rads.</returns>
        public static double GetAngle(MEPCurve mEPCurve1, MEPCurve mEPCurve2)
        {
            XYZ vector1 = GetDirection(mEPCurve1);
            XYZ vector2 = GetDirection(mEPCurve2);

            return vector1.AngleTo(vector2);
        }

        /// <summary>
        /// Get plane by two MEPCurves
        /// </summary>
        /// <param name="mEPCurve1"></param>
        /// <param name="mEPCurve2"></param>
        /// <returns>Return plane.</returns>
        public static Plane GetPlane(MEPCurve mEPCurve1, MEPCurve mEPCurve2)
        {
            var locCurve1 = mEPCurve1.Location as LocationCurve;
            var line1 = locCurve1.Curve as Line;

            var locCurve2 = mEPCurve2.Location as LocationCurve;
            var line2 = locCurve2.Curve as Line;

            List<XYZ> planePoints = new List<XYZ>()
            {
                line1.Origin,
                line1.GetEndPoint(0),
                line1.GetEndPoint(1),
                line2.Origin,
                line2.GetEndPoint(0),
                line2.GetEndPoint(1),
            };

            List<XYZ> difPlanePoints = GetNotEqualPoints(planePoints);

            if (difPlanePoints.Count < 3)
            {
                return null;
            }

            return Plane.CreateByThreePoints(difPlanePoints[0], difPlanePoints[1], difPlanePoints[2]);
        }

        private static List<XYZ> GetNotEqualPoints(List<XYZ> planePoints)
        {
            List<XYZ> difPlanePoints = new List<XYZ>()
            {
                planePoints[0]
            };

            foreach (var point in planePoints)
            {
                if (IsPointContainsInList(difPlanePoints, point))
                {
                    continue;
                }

                difPlanePoints.Add(point);
            }

            return difPlanePoints;
        }

        private static bool IsPointContainsInList(List<XYZ> points, XYZ point)
        {
            foreach (var p in points)
            {
                if (point.DistanceTo(p) < 0.001)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if mEPCurve1 direction or negate direction is equal to mEPCurve2 direction.
        /// </summary>
        /// <param name="mEPCurve1"></param>
        /// <param name="mEPCurve2"></param>
        /// <returns>Return true if directions are equal.</returns>
        public static bool IsDirectionEqual(MEPCurve mEPCurve1, MEPCurve mEPCurve2)
        {
            if (GetDirection(mEPCurve1).IsAlmostEqualTo(GetDirection(mEPCurve2)) ||
                GetDirection(mEPCurve1).Negate().IsAlmostEqualTo(GetDirection(mEPCurve2)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get MEPCurve's size by vector of MEPCurve's center point.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="normVector"></param>
        /// <returns>Return double length between MEPCurve's center point and intersection point between vector and MEPCurve's solid.</returns> 
        public static double GetSizeByVector(MEPCurve mEPCurve, XYZ normVector)
        {
            List<Solid> elemSolids = ElementUtils.GetSolids(mEPCurve);
            Solid elemSolid = elemSolids.First();

            XYZ centerPoint = ElementUtils.GetLocationPoint(mEPCurve);

            var locCurve = mEPCurve.Location as LocationCurve;
            Line mEPCurveline = locCurve.Curve as Line;
            Line intersectLine = Line.CreateBound(centerPoint, centerPoint + normVector.Multiply(100));

            SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();
            SolidCurveIntersection intersection = elemSolid.IntersectWithCurve(intersectLine, intersectOptions);

            XYZ intersectionPoint = null;
            if (intersection.SegmentCount != 0)
            {
                XYZ p1 = intersection.GetCurveSegment(0).GetEndPoint(0);
                XYZ p2 = intersection.GetCurveSegment(0).GetEndPoint(1);

                (XYZ minPoint, XYZ maxPoint) = XYZUtils.GetMinMaxPoints(new List<XYZ> { p1, p2 }, mEPCurveline);
                intersectionPoint = maxPoint;
            }

            return 2 * mEPCurveline.Distance(intersectionPoint);
        }

        /// <summary>
        /// Check if MEPCurves are equal oriented.
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <param name="mEPCurve"></param>
        /// <returns>Return true if MEPCurves are equal oriented.</returns>
        public static bool EqualOriented(MEPCurve baseMEPCurve, MEPCurve mEPCurve)
        {
            Basis baseBasis = baseMEPCurve.GetBasis();
            Basis curveBasis = mEPCurve.GetBasis();
            return XYZUtils.Collinearity(baseBasis.Y, curveBasis.Y) || XYZUtils.Collinearity(baseBasis.Z, curveBasis.Z);
        }

        /// <summary>
        /// Get vector which direction is not parallel to mEPCurve's direction.
        /// </summary>
        /// <param name="baseNorms"></param>
        /// <param name="dir"></param>
        /// <returns>Return vector which direction is not parallel to mEPCurve's direction.</returns>
        private static XYZ GetMesureVector(List<XYZ> baseNorms, XYZ dir)
        {
            foreach (var norm in baseNorms)
            {
                if (!XYZUtils.Collinearity(dir, norm))
                {
                    return norm;
                }
            }

            return null;
        }

        private static List<XYZ> GetVectorsInPlane(Plane plane, List<XYZ> vectors)
        {
            List<XYZ> planeVectors = new List<XYZ>();
            foreach (var vector in vectors)
            {
                XYZ cpVector = plane.Origin + vector;
                if (cpVector.IsPointOntoPlane(plane))
                {
                    planeVectors.Add(vector);
                }
            }

            return planeVectors;
        }

        public static ConnectorProfileType GetProfileType(MEPCurve mEPCurve)
        {
            var doc = mEPCurve.Document;
            var type = doc.GetElement(mEPCurve.GetTypeId()) as MEPCurveType;
            return type.Shape;
        }

        /// <summary>
        /// Get sized of MEPCurve.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Returns actual sized in recrangle case and diameter in round case.</returns>
        public static (double width, double heigth) GetWidthHeight(MEPCurve mEPCurve)
        {
            double width = 0;
            double heigth = 0;

            ConnectorProfileType connectorProfileType = GetProfileType(mEPCurve);
            switch (connectorProfileType)
            {
                case ConnectorProfileType.Invalid:
                    break;
                case ConnectorProfileType.Round:
                    {
                        width = mEPCurve.Diameter;
                        heigth = width;
                    }
                    break;
                case ConnectorProfileType.Rectangular:
                    {
                        width = mEPCurve.Width;
                        heigth = mEPCurve.Height;
                    }
                    break;
                case ConnectorProfileType.Oval:
                    break;
                default:
                    break;
            }

            return (width, heigth);
        }

        /// <summary>
        /// Get cross-sectional area of MEPCurve.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns></returns>
        public static double GetCrossSectionArea(MEPCurve mEPCurve)
        {
            var doc = mEPCurve.Document;

            var type = doc.GetElement(mEPCurve.GetTypeId()) as MEPCurveType;
            var shape = type.Shape;
            string typeName = mEPCurve.GetType().Name;

            double area = 0;

            switch (shape)
            {
                case ConnectorProfileType.Invalid:
                    break;
                case ConnectorProfileType.Round:
                    double d;
                    if (typeName == "Pipe")
                    {
                        d = mEPCurve.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();
                    }
                    else
                    {
                        d = mEPCurve.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsDouble();
                    }
                    area = Math.PI * Math.Pow(d, 2) / 4;
                    break;
                case ConnectorProfileType.Rectangular:
                    double width = mEPCurve.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble();
                    double height = mEPCurve.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble();
                    area = width * height;
                    break;
                case ConnectorProfileType.Oval:
                    break;
            }

            return area;
        }

        /// <summary>
        /// Check if connected element is root element.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="element"></param>
        /// <returns>Return true if element is not child spud.</returns>
        public static bool IsRoot(MEPCurve mEPCurve, Element element)
        {
            if (!ConnectorUtils.ElementsConnected(mEPCurve, element))
            {
                var s = "Elements aren't connected";
                throw new ArgumentException(s);
            }

            var mEPCurveDir = GetDirection(mEPCurve);

            var mEPCurveLP = ElementUtils.GetLocationPoint(mEPCurve);
            var elemLP = ElementUtils.GetLocationPoint(element);

            XYZ vector = (mEPCurveLP - elemLP).RoundVector().Normalize();

            if (XYZUtils.Collinearity(mEPCurveDir, vector))
            {
                return true;
            }

            return false;
        }

        public static List<Element> GetOrderedConnected(MEPCurve mEPCurve, XYZ basePoint)
        {
            var connectedElems = ConnectorUtils.GetConnectedElements(mEPCurve);
            if (connectedElems is null || !connectedElems.Any())
            {
                return connectedElems;
            }

            return connectedElems.OrderByPoint(basePoint);
        }

        /// <summary>
        /// Get not spud connectors of MEPCurve.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns></returns>
        public static List<Connector> GetNotSpudConnectors(MEPCurve mEPCurve)
        {
            var connectedElems = ConnectorUtils.GetConnectedElements(mEPCurve);

            if (connectedElems is null || !connectedElems.Any())
            {
                var connectors = ConnectorUtils.GetConnectors(mEPCurve);
                return connectors;
            }

            var notSpudElements = connectedElems.ExludeSpudes();

            if (notSpudElements is not null && notSpudElements.Any())
            {
                var cons = new List<Connector>();
                foreach (var elem in notSpudElements)
                {
                    var (elem1Con, elem2Con) = ConnectorUtils.GetCommonConnectors(elem, mEPCurve);
                    cons.Add(elem2Con);
                }
                return cons;
            }
            else
            {
                var freeCons = ConnectorUtils.GetFreeConnector(mEPCurve);
                return freeCons;
            };
        }

        public static MEPCurve GetMaxLengthMEPCurve(List<MEPCurve> mEPCurves)
        {
            double maxLength = 0;
            MEPCurve maxLengthMEPCurve = mEPCurves.First();
            if (mEPCurves.Count == 1)
            {
                return maxLengthMEPCurve;
            }

            for (int i = 1; i < mEPCurves.Count; i++)
            {
                double l = MEPCurveUtils.GetLength(mEPCurves[i]);
                if (l > maxLength)
                {
                    maxLength = l;
                    maxLengthMEPCurve = mEPCurves[i];
                }

            }
            return maxLengthMEPCurve;
        }

        /// <summary>
        /// Align <paramref name="operationMEPCurve"/> of rectangular profile with 
        /// <paramref name="sourceMEPCurve"/> by reference direction rotation.
        /// </summary>
        /// <param name="sourceMEPCurve"></param>
        /// <param name="operationMEPCurve"></param>
        public static void AlignMEPCurve(MEPCurve sourceMEPCurve, MEPCurve operationMEPCurve)
        {
            if (!sourceMEPCurve.IsRectangular())
                return;

            //Check if rect is needed to align
            Basis baseBasis = sourceMEPCurve.GetBasis();
            Basis curveBasis = operationMEPCurve.GetBasis();
            if (XYZUtils.Collinearity(baseBasis.Y, curveBasis.Y) || XYZUtils.Collinearity(baseBasis.Z, curveBasis.Z))
            {
                return;
            }

            //get align options
            XYZ cross = baseBasis.X.CrossProduct(curveBasis.X);
            (XYZ baseAlignBasis, XYZ alignBasis) = XYZUtils.Collinearity(baseBasis.Y, cross) ?
                 (baseBasis.Y, curveBasis.Y) :
                 (baseBasis.Z, curveBasis.Z);
            //(XYZ baseAlignBasis, XYZ alignBasis) = GetAlignBasis(baseBasis, curveBasis, cross);
            double angle = baseAlignBasis.AngleTo(alignBasis);
            Line axis = operationMEPCurve.GetCenterLine();
            //Line axis = Line.CreateUnbound(curveBasis.Point, curveBasis.X);
            var basis = new Basis(axis.Direction, baseAlignBasis, alignBasis, axis.Origin);

            BasisOrientation orientation = basis.GetOrientaion();
            angle = orientation == BasisOrientation.Left ? angle : -angle;

            //align

            ElementTransformUtils.RotateElement(sourceMEPCurve.Document, operationMEPCurve.Id, axis, angle);
        }


        /// <summary>
        /// Align <see cref="Basis"/>.Y of <paramref name="sourceMEPCurve"/> with 
        /// <paramref name="targetBasis"/> <see cref="Basis"/>.Y.
        /// </summary>
        /// <param name="sourceMEPCurve"></param>
        /// <param name="targetBasis"></param>
        /// <returns>Returns rotation <see cref="Transform"/> to align Y <see cref="Basis"/>'s. </returns>
        public static Transform GetAlignTransform(MEPCurve sourceMEPCurve, Basis targetBasis)
        {
            if (!sourceMEPCurve.IsRectangular())
                return null;

            //Check if rect is needed to align
            Basis sourceBasis = sourceMEPCurve.GetBasis();
            if (XYZUtils.Collinearity(sourceBasis.Y, targetBasis.Y) || XYZUtils.Collinearity(sourceBasis.Z, targetBasis.Z))
            { return null;}

            //get align options
            XYZ cross = sourceBasis.X.CrossProduct(targetBasis.X).Normalize();
            XYZ crossY = sourceBasis.Y.CrossProduct(targetBasis.Y).Normalize();
            (XYZ sourceAlignBasis, XYZ targetBasisToAlign) = XYZUtils.Collinearity(targetBasis.Y, cross) ?
                 (sourceBasis.Y, targetBasis.Y) :
                 (sourceBasis.Z, targetBasis.Z);
            double angle = sourceBasis.Y.AngleOnPlaneTo(targetBasis.Y, crossY);
            //double angle = sourceAlignBasis.AngleTo(targetBasisToAlign);
            var basis = new Basis(targetBasis.X, sourceAlignBasis, targetBasisToAlign, targetBasis.Point);

            BasisOrientation orientation = basis.GetOrientaion();
            angle = orientation == BasisOrientation.Left ? angle : -angle;

            //align
            return Transform.CreateRotationAtPoint(basis.X, angle, basis.Point);
        }

        /// <summary>
        /// Get <see cref="Autodesk.Revit.DB.FamilyInstance"/> between 
        /// <paramref name="mEPCurve1"/> and <paramref name="mEPCurve2"/>.
        /// </summary>
        /// <param name="mEPCurve1"></param>
        /// <param name="mEPCurve2"></param>
        /// <returns>Returns <see cref="Autodesk.Revit.DB.FamilyInstance"/> 
        /// if it's connected both to <paramref name="mEPCurve1"/> and <paramref name="mEPCurve2"/>.
        /// Otherwise returns <see langword="null"/>.
        /// </returns>
        public static FamilyInstance GetJunction(MEPCurve mEPCurve1, MEPCurve mEPCurve2)
        {
            if(mEPCurve1.Id == mEPCurve2.Id) { return null; }

            var elems1 = ConnectorUtils.GetConnectedElements(mEPCurve1);
            var elems2 = ConnectorUtils.GetConnectedElements(mEPCurve2);
            ElementId intersectionElementId =  elems1.Select(obj => obj.Id).
                Intersect(elems2.Select(obj => obj.Id)).
                FirstOrDefault();

            return intersectionElementId is null ? 
                null : 
                mEPCurve1.Document.GetElement(intersectionElementId) as FamilyInstance;
        }
    }

}
