using DS.RVT.ModelSpaceFragmentation.Points;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class WaveExpander
    {       
        public static StepPoint StartStepPoint;
        public static StepPoint EndStepPoint;

        public static Dictionary<StepPoint, int> Grid { get; set; }
        private static List<StepPoint> initialPriorityList;
        private static List<StepPoint> clzPoints;

        public static bool Expand(ISpacePointsIterator spacePointsIterator)
        {
            FillData();
            Iterate(spacePointsIterator);

            if (!Grid.ContainsKey(EndStepPoint))
                return false;

            return true;
        }

        static void FillData()
        {

            StartStepPoint = new StepPoint(InputData.Ax, InputData.Ay, InputData.Az);
            EndStepPoint = new StepPoint(InputData.Bx, InputData.By, InputData.Bz);

            CLZCretor clzCreator = new CLZCretor();
            clzPoints = clzCreator.Create(new CLZByBox());

            List<StepPoint> stepPointsList = new List<StepPoint>();
            stepPointsList.Add(StartStepPoint);


            Priority PriorityInstance = new Priority();
            initialPriorityList = PriorityInstance.GetPriorities();
        }

        static void Iterate(ISpacePointsIterator iteratorByPlane)
        {
            Grid = new Dictionary<StepPoint, int>
            {
                { StartStepPoint, 1 }
            };
            iteratorByPlane.Iterate();
        }

        public static bool Operation(int x, int y, int z)
        {
            StepPoint currentPoint = new StepPoint(x, y, z);

            int currentD;
            if (!Grid.ContainsKey(currentPoint))
                return false;
            else
                currentD = Grid[currentPoint];

            //Create points cloud for next check from unpassible points
            PointsCloud pointsCloud = new PointsCloud(SpaceFragmentator.UnpassablePoints);
            List<StepPoint> unpassableByCLZPoints =
                pointsCloud.GetStepPointByCenterPoint(PointConvertor.StepPointToXYZ(currentPoint));

            NeighboursPasser neighboursPasser =
                new NeighboursPasser(currentPoint, clzPoints, unpassableByCLZPoints, initialPriorityList, currentD);
            neighboursPasser.Pass();

            return true;
        }
    }
}
