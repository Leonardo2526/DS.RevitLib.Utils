using System.Collections.Generic;
using DS.RVT.ModelSpaceFragmentation.Path.Neighbours;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class NeighboursPasser
    {
        public NeighboursPasser(StepPoint currentPoint,
            List<StepPoint> cLZPoints, List<StepPoint> unpassableByCLZPoints, List<StepPoint> initialPriorityList,
            int currentD)
        {
            CurrentPoint = currentPoint;
            CLZPoints = cLZPoints;
            UnpassableByCLZPoints = unpassableByCLZPoints;
            InitialPriorityList = initialPriorityList;
            CurrentD = currentD;
        }

        public static StepPoint CurrentPoint { get; set; }
        public static List<StepPoint> CLZPoints { get; set; }
        public static List<StepPoint> UnpassableByCLZPoints { get; set; }
        public List<StepPoint> InitialPriorityList { get; set; }
        public static int CurrentD { get; set; }
        public static int D { get; set; }
        public static int A { get; set; }

        public void Pass()
        {
            if (PathFinder.spacePointsIterator is IteratorByXYPlane)
                SetPassOption(new XYINeighboursPasser());
            else if (PathFinder.spacePointsIterator is IteratorByXZPlane)
                SetPassOption(new XZNeighboursPasser());
            else if (PathFinder.spacePointsIterator is IteratorByYZPlane)
                SetPassOption(new YZNeighboursPasser());
            else if (PathFinder.spacePointsIterator is IteratorBy3D)
                SetPassOption(new AllNeighboursPasser());
        }

        private void SetPassOption(INeighboursPasser markPointPasser)
        {
            markPointPasser.Pass();
        }

      

        public void MarkOld(ref int d, ref int a)
        {
            int k;
            // проходим по всем непомеченным соседям
            for (k = 0; k < 6; ++k)
            {
                int ix = CurrentPoint.X + InitialPriorityList[k].X,
                    iy = CurrentPoint.Y + InitialPriorityList[k].Y,
                    iz = CurrentPoint.Z + InitialPriorityList[k].Z;

                if (ix >= 0 && ix < InputData.Xcount &&
                    iy >= 0 && iy < InputData.Ycount &&
                    iz >= 0 && iz < InputData.Zcount)
                {
                    StepPoint nextPoint = new StepPoint(ix, iy, iz);

                    GridPointChecker gridPointChecker =
                        new GridPointChecker(nextPoint);
                    gridPointChecker.Check();
                }
            }
        }
    }
}
