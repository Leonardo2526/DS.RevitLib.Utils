using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class LineCollision
    {
        SolidCurveIntersectionOptions intersectOptions;

        public LineCollision(SolidCurveIntersectionOptions intersectOptions)
        {
            this.intersectOptions = intersectOptions;
        }

        public bool GetElementsCurveCollisions(Curve curve, List<Solid> solids)
        {
           
                foreach (Solid solid in solids)
                {
                    //Get intersections with curve
                    SolidCurveIntersection intersection = solid.IntersectWithCurve(curve, intersectOptions);
               
                    if (intersection.SegmentCount != 0)
                    {
                        CurveExtents curveExt = intersection.GetCurveSegmentExtents(0);
                        if (curveExt.StartParameter == 0)
                            return true;
                    }
                }           
            return false;
        }

        public bool GetElementsCurveCollisionsOld(Curve curve, Dictionary<Element, List<Solid>> elementsSolids)
        {
            foreach (KeyValuePair<Element, List<Solid>> keyValue in elementsSolids)
            {
                foreach (Solid solid in keyValue.Value)
                {
                    //Get intersections with curve
                    SolidCurveIntersection intersection = solid.IntersectWithCurve(curve, intersectOptions);
                    if (intersection.SegmentCount != 0)
                    {
                        CurveExtents curveExt = intersection.GetCurveSegmentExtents(0);
                        if (curveExt.StartParameter == 0)
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
