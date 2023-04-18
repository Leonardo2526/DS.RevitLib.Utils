using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitLib.Utils.Solids
{

    /// <summary>
    /// Toolf for 'Solid' object.
    /// </summary>
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
            solids = solids.Where(obj => obj is not null).ToList();
            double minVolumeCm = UnitUtils.ConvertToInternalUnits(minVolume, DisplayUnitType.DUT_CUBIC_CENTIMETERS);

            Solid initialSolid = solids.FirstOrDefault();
            for (int i = 1; i < solids.Count; i++)
            {
                if (solids[i].Volume < minVolumeCm)
                {
                    continue;
                }
                try
                {
                    initialSolid = BooleanOperationsUtils.ExecuteBooleanOperation(solids[i], initialSolid, BooleanOperationsType.Union);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception message: Failed to unite the solids");   
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
                    //Debug.WriteLine(ex);
                    //TaskDialog.Show("Error", "Failed to find intersection between solids. \n" + ex.Message); 
                }
            }

            return null;
        }


        /// <summary>
        /// Get solid's size by vector from center point of solid. 
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="normVector"></param>
        /// <param name="solidCentroid"></param>
        /// <returns>Returns distance between two points of intersection between solid and line by vector.</returns>
        public static double GetSizeByVector(Solid solid, XYZ normVector, XYZ solidCentroid = null)
        {
            solidCentroid ??= solid.ComputeCentroid();
            Line intersectLine = Line.CreateBound(solidCentroid, solidCentroid + normVector.Multiply(100));

            SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();
            SolidCurveIntersection intersection = solid.IntersectWithCurve(intersectLine, intersectOptions);

            if (intersection.SegmentCount != 0)
            {
                XYZ p1 = intersection.GetCurveSegment(0).GetEndPoint(0);
                XYZ p2 = intersection.GetCurveSegment(0).GetEndPoint(1);

                return p1.DistanceTo(p2);
            }

            return 0;
        }

        /// <summary>
        /// Get min and max solid's sizes by vector from center point of solid. 
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="normVector"></param>
        /// <returns>Returns distance between center point and points of intersection between solid and line by vector.</returns>
        public static (double, double) GetMinMaxSizeByVector(Solid solid, XYZ normVector)
        {
            XYZ centerPoint = solid.ComputeCentroid();
            Line intersectLine = Line.CreateBound(centerPoint, centerPoint + normVector.Multiply(100));

            SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();
            SolidCurveIntersection intersection = solid.IntersectWithCurve(intersectLine, intersectOptions);

            if (intersection.SegmentCount != 0)
            {
                XYZ p1 = intersection.GetCurveSegment(0).GetEndPoint(0);
                XYZ p2 = intersection.GetCurveSegment(0).GetEndPoint(1);

                double p1_Center = p1.DistanceTo(centerPoint);
                double p2_Center = p2.DistanceTo(centerPoint);
                return (Math.Max(p1_Center, p2_Center), Math.Min(p1_Center, p2_Center));
            }

            return (0, 0);
        }

        /// <summary>
        /// Get solid of intersections with element1 and all connected to element2.
        /// </summary>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        /// <param name="intersectionSolid"></param>
        /// <returns>Returns united solid from all intersections.</returns>
        public static Solid GetGroupIntersectionSolid(Element element1, Element element2, Solid intersectionSolid)
        {
            //Get connected to noBandable
            var connectedToNoband = ConnectorUtils.GetAllConnectedWithCollisions(element2, element1, element2.Document);

            if (connectedToNoband.Count == 0)
            {
                return intersectionSolid;
            }

            IList<Element> collisionElements = DS.RevitLib.Utils.CollisionUtils.GetByElements(element1, connectedToNoband, new List<Element>() { element2 });

            if (collisionElements.Count == 0)
            {
                return intersectionSolid;
            }

            List<Solid> solidIntersections = GetIntersection(new List<Element>() { element1 }, collisionElements.ToList());
            solidIntersections.Add(intersectionSolid);

            Solid resusltSolid = UniteSolids(solidIntersections, 1);

            return resusltSolid;
        }

        /// <summary>
        /// Get norm vectors of solid from it's faces.
        /// </summary>
        /// <param name="solid"></param>
        /// <returns>Returns norm vectors of element.</returns>
        public static List<XYZ> GetOrhts(Solid solid)
        {
            var vectors = new List<XYZ>();
            var faces = solid.Faces;

            foreach (Face face in faces)
            {
                XYZ vector = face.ComputeNormal(UV.Zero);
                vectors.Add(vector);
            }

            return vectors;
        }

        /// <summary>
        /// Get norm otho vectors of solid from it's faces in perpendicular plane to solid's centralLine.
        /// </summary>
        /// <param name="solid"></param>
        /// <returns>Returns norm ortho vectors of element.</returns>
        public static List<XYZ> GetOrthoNormVectors(Solid solid, Line centralLine)
        {
            var orthoVectors = new List<XYZ>();
            var vectors = GetOrhts(solid);

            foreach (var vector in vectors)
            {
                if (!XYZUtils.Collinearity(vector, centralLine.Direction))
                {
                    orthoVectors.Add(vector);
                }
            }

            return orthoVectors;
        }

    }
}
