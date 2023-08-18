using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RVT.ModelSpaceFragmentation.Path.CLZ;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class CLZByBox : IZonePoints
    {
   

        public List<StepPoint> Create()
        {
            List<StepPoint> ZonePoints = new List<StepPoint>();

            for (int z = -CLZInfo.FullDistanceInSteps; z <= CLZInfo.FullDistanceInSteps; z++)
            {
                for (int y = -CLZInfo.FullDistanceInSteps; y <= CLZInfo.FullDistanceInSteps; y++)
                {
                    for (int x = -CLZInfo.FullDistanceInSteps; x <= CLZInfo.FullDistanceInSteps; x++)
                    {
                        if (x == 0 && y == 0 && z == 0)
                            continue;

                        StepPoint stepPoint = new StepPoint(x, y, z);
                        ZonePoints.Add(stepPoint);
                    }

                }
            }
            return ZonePoints;
        }
    }
}