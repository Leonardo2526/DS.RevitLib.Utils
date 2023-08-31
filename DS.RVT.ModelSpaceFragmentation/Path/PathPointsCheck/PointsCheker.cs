using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using DS.RVT.ModelSpaceFragmentation.Points;
using DS.RVT.ModelSpaceFragmentation.Path.CLZ;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class PointsCheker
    {
        public bool IsPointPassable(StepPoint point)
        {
            if (InputData.UnpassStepPoints.Count == 0)
                return true;

            if (InputData.UnpassStepPoints.Contains(point))
                return false;

            return true;
        }

        public bool IsClearanceZoneAvailable(StepPoint stepPoint)
        {
            if (NeighboursPasser.UnpassableByCLZPoints.Count == 0)
                return true;

            foreach (StepPoint clearancePoint in NeighboursPasser.CLZPoints)
            {
                StepPoint currentPoint = new StepPoint(
                    stepPoint.X + clearancePoint.X,
                    stepPoint.Y + clearancePoint.Y,
                    stepPoint.Z + clearancePoint.Z
                    );
                if (NeighboursPasser.UnpassableByCLZPoints.Contains(currentPoint))
                    return false;
            }

            return true;
        }

        public static bool IsPointPassableByMark(StepPoint stepPoint)
        {
            XYZ XYZpoint = PointConvertor.StepPointToXYZ(stepPoint);
            foreach (XYZ upassPoint in SpaceFragmentator.UnpassablePoints)
            {
                if (XYZpoint.DistanceTo(upassPoint) <= CLZInfo.FullDistanceWithMarkPointF)
                    return false;
            }

            return true;
        }
    }
}
