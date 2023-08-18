using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Lines;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PointInSolidChecker
    {
        readonly LineCreator lineCreator;
        readonly LineCollision lineCollision;

        public PointInSolidChecker(LineCreator lineCreator, LineCollision lineCollision)
        {
            this.lineCreator = lineCreator;
            this.lineCollision = lineCollision;

        }

        public bool IsPointInSolid(XYZ point, List<Solid> solids)
        {
            RayCreator ray = new RayCreator(point);
            Line rayLine = lineCreator.Create(ray);

            bool IfOneLineIntersections = lineCollision.GetElementsCurveCollisions(rayLine, solids);
            if (IfOneLineIntersections)
                return true;

            return false;
        }
    }
}
