namespace DS.RVT.ModelSpaceFragmentation.Path.Neighbours
{
    class YZNeighboursPasser : INeighboursPasser
    {
        public void Pass()
        {
            foreach (StepPoint stepPoint in StepPointsList.YZPoints)
                NeighboursMarker.Mark(stepPoint);
        }
    }
}
