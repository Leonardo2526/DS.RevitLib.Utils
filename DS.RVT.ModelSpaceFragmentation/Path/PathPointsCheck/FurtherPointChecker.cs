using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class FurtherPointChecker
    {
        public static bool IsClearanceAvailable(StepPoint stepPoint, StepPoint BackWayStepPoint, 
            Dictionary<StepPoint, int> grid)
        {
            StepPoint nextPoint;

            if (StepsPriority.CurrentPriority == WaveAlgorythm.InitialPriority)
                return true;

            if (StepsPriority.CurrentPriority == 1)
            {
                for (int i = 0; i <= ZoneByCircle.FullZoneCleranceInSteps; i++)
                {
                    nextPoint = new StepPoint(stepPoint.X + BackWayStepPoint.X * (1 + i), stepPoint.Y, stepPoint.Z);
                    if (!grid.ContainsKey(nextPoint))
                        return false;
                }
            }
            else if (StepsPriority.CurrentPriority == 2)
            {
                for (int i = 0; i <= ZoneByCircle.FullZoneCleranceInSteps; i++)
                {
                    nextPoint = new StepPoint(stepPoint.X, stepPoint.Y + BackWayStepPoint.Y * (1 + i), stepPoint.Z);
                    if (!grid.ContainsKey(nextPoint))
                        return false;
                }
            }
            else if (StepsPriority.CurrentPriority == 3)
            {
                for (int i = 0; i <= ZoneByCircle.FullZoneCleranceInSteps; i++)
                {
                    nextPoint = new StepPoint(stepPoint.X, stepPoint.Y, stepPoint.Z + BackWayStepPoint.Z * (2 + i));
                    if (!grid.ContainsKey(nextPoint))
                        return false;
                }
            }

            return true;
        }
    }
}
