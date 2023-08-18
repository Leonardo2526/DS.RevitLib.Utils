using System;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class Priority
    {
        public List<StepPoint> GetPriorities()
        {
            StepsPriority stepsPriority = new StepsPriority();

            if (Math.Abs(ElementInfo.StartElemPoint.X - ElementInfo.EndElemPoint.X) < 0.01)
                return stepsPriority.GetPointsList(2);
            else
                return stepsPriority.GetPointsList(1);
        }

      
        public List<StepPoint> GetPrioritiesByPoint(StepPoint currentPoint, StepPoint endPoint)
        {
            StepsPriority stepsPriority = new StepsPriority();

            if (Math.Abs(ElementInfo.StartElemPoint.Z - ElementInfo.EndElemPoint.Z) < 0.01 &&
            Math.Abs(ElementInfo.StartElemPoint.Y - ElementInfo.EndElemPoint.Y) < 0.01)
            {
                if (currentPoint.Y != endPoint.Y)
                    return stepsPriority.GetPointsList(2);
                else if (currentPoint.Z != endPoint.Z)
                    return stepsPriority.GetPointsList(3);
                else
                    return stepsPriority.GetPointsList(1);
            }
            else if (Math.Abs(ElementInfo.StartElemPoint.Z - ElementInfo.EndElemPoint.Z) < 0.01 &&
            Math.Abs(ElementInfo.StartElemPoint.X - ElementInfo.EndElemPoint.X) < 0.01)
            {
                if (currentPoint.X != endPoint.X)
                    return stepsPriority.GetPointsList(1);
                else if (currentPoint.Z != endPoint.Z)
                    return stepsPriority.GetPointsList(3);
                else
                    return stepsPriority.GetPointsList(2);
            }

            if (Math.Abs(ElementInfo.StartElemPoint.X - ElementInfo.EndElemPoint.X) < 0.01 && currentPoint.X != endPoint.X)
                return stepsPriority.GetPointsList(1);

            if (Math.Abs(ElementInfo.StartElemPoint.Y - ElementInfo.EndElemPoint.Y) < 0.01 && currentPoint.Y != endPoint.Y)
                return stepsPriority.GetPointsList(2);

            return stepsPriority.GetPointsList(1);
        }
    }
}
