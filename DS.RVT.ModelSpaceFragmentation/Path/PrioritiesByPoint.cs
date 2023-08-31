using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class PrioritiesByPoint
    {
        public PrioritiesByPoint(StepPoint currentPoint, StepPoint endPoint, StepPoint backWayStepPoint, Dictionary<StepPoint, int> grid)
        {
            CurrentPoint = currentPoint;
            EndPoint = endPoint;
            BackWayStepPoint = backWayStepPoint;
            Grid = grid;
        }

        StepPoint CurrentPoint { get; set; }
        StepPoint EndPoint { get; set; }
        StepPoint BackWayStepPoint { get; set; }
        Dictionary<StepPoint, int> Grid { get; set; }


        public List<StepPoint> GetPrioritiesByPoint()
        {
            StepsPriority stepsPriority = new StepsPriority();

                if (Math.Abs(ElementInfo.StartElemPoint.Z - ElementInfo.EndElemPoint.Z) < 0.01 &&
                Math.Abs(ElementInfo.StartElemPoint.Y - ElementInfo.EndElemPoint.Y) < 0.01)
                {
                    if (CurrentPoint.Y != EndPoint.Y)
                        return stepsPriority.GetPointsList(2);
                    else if (CurrentPoint.Z != EndPoint.Z)
                        return stepsPriority.GetPointsList(3);
                    else
                        return stepsPriority.GetPointsList(1);
                }
                else if (Math.Abs(ElementInfo.StartElemPoint.Z - ElementInfo.EndElemPoint.Z) < 0.01 &&
                Math.Abs(ElementInfo.StartElemPoint.X - ElementInfo.EndElemPoint.X) < 0.01)
                {
                    if (CurrentPoint.X != EndPoint.X)
                        return stepsPriority.GetPointsList(1);
                    else if (CurrentPoint.Z != EndPoint.Z)
                        return stepsPriority.GetPointsList(3);
                    else
                        return stepsPriority.GetPointsList(2);
                }

                if (Math.Abs(ElementInfo.StartElemPoint.X - ElementInfo.EndElemPoint.X) < 0.01 &&
                    CurrentPoint.X != EndPoint.X)
                    return stepsPriority.GetPointsList(1);

                if (Math.Abs(ElementInfo.StartElemPoint.Y - ElementInfo.EndElemPoint.Y) < 0.01 &&
                    CurrentPoint.Y != EndPoint.Y)
                    return stepsPriority.GetPointsList(2);
          


            return stepsPriority.GetPointsList(1);
        }

    }
}
