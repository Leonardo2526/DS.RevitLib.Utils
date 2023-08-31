using DS.RVT.ModelSpaceFragmentation.Path.CLZ;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class CLZByBoders : IZonePoints
    {
        readonly List<StepPoint> ZonePoints = new List<StepPoint>();


        public List<StepPoint> Create()
        {
            int dist = CLZInfo.FullDistanceInSteps;

            for (int z = -dist; z <= dist; z += 2 * dist)
            {
                for (int y = -dist; y <= dist; y += 2 * dist)
                {
                    for (int x = -dist; x <= dist; x++)
                    {
                        AddPoint(x, y, z);
                        AddPoint(y, x, z);
                        AddPoint(z, y, x);
                    }
                }
            }

            return ZonePoints;
        }

        void AddPoint(int x, int y, int z)
        {
            StepPoint stepPoint = new StepPoint(x, y, z);

            if (!ZonePoints.Contains(stepPoint))
            ZonePoints.Add(stepPoint);
        }
    }
}