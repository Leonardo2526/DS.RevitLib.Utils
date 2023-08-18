using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class GridPointChecker
    {
        public GridPointChecker(StepPoint nextPoint)
        {
            NextPoint = nextPoint;
        }

        public StepPoint NextPoint { get; set; }

        public void Check()
        {
            PointsCheker pointsCheker = new PointsCheker();

            if (!WaveExpander.Grid.ContainsKey(NextPoint))
            {

                bool checkUnpassablePoint = pointsCheker.IsPointPassable(NextPoint);
                bool checkClearancePoint = pointsCheker.IsClearanceZoneAvailable(NextPoint);
                if (checkUnpassablePoint && checkClearancePoint)
                    {
                    // распространяем волну
                    NeighboursPasser.D = NeighboursPasser.CurrentD + 1;
                    WaveExpander.Grid.Add(NextPoint, NeighboursPasser.D);
                    NeighboursPasser.A++;
                }
            }
        }
    }
}
