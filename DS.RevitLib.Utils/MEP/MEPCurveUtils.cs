using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Solids;
using Ivanov.RevitLib.Utils;
using System;
using System.Collections;
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
        /// Get norm vectors of MEPCurve from it's faces.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Returns norm vectors of MEPCurve.</returns>
        public static List<XYZ> GetNormVectors(MEPCurve mEPCurve)
        {
            var vectors = new List<XYZ>();
            var faces = ElementUtils.GetFaces(mEPCurve);

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
        /// Get norm otho vectors of MEPCurve from it's faces in perpendicular plane to MEPCurve's direction.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <returns>Returns norm ortho vectors of MEPCurve.</returns>
        public static List<XYZ> GetOrthoNormVectors(MEPCurve mEPCurve)
        {
            XYZ dir = GetDirection(mEPCurve);

            var orthoVectors = new List<XYZ>();
            var vectors = GetNormVectors(mEPCurve);

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

                (XYZ minPoint, XYZ maxPoint) = PointUtils.GetMinMaxPoints(new List<XYZ> { p1, p2 }, mEPCurveline);
                intersectionPoint = maxPoint;
            }

            return 2 * mEPCurveline.Distance(intersectionPoint);
        }

        /// <summary>
        /// Check if MEPCurves are equal oriented. 
        /// Check if size by one of baseMEPCurve norm vector is equal to mEPCurve size by the same vector.
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <param name="mEPCurve"></param>
        /// <returns>Return true if MEPCurves are equal oriented.</returns>
        public static bool EqualOriented(MEPCurve baseMEPCurve, MEPCurve mEPCurve)
        {
            XYZ baseDir = GetDirection(baseMEPCurve);
            XYZ dir = GetDirection(mEPCurve);

            List<XYZ> baseNorms = GetOrthoNormVectors(baseMEPCurve);

            XYZ measureVector = GetMesureVector(baseNorms, dir, baseDir);

            double baseSize = GetSizeByVector(baseMEPCurve, measureVector);
            double size = GetSizeByVector(mEPCurve, measureVector);

            if (Math.Abs(baseSize - size) < 0.001)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get vector which direction is not parallel to mEPCurve's direction.
        /// </summary>
        /// <param name="baseNorms"></param>
        /// <param name="dir"></param>
        /// <returns>Return vector which direction is not parallel to mEPCurve's direction.</returns>
        private static XYZ GetMesureVector(List<XYZ> baseNorms, XYZ dir, XYZ baseDir)
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
        public static (double, double) GetWidthHeight(MEPCurve mEPCurve)
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
    }
}
