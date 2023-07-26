namespace DS.RVT.ModelSpaceFragmentation.Path.Neighbours
{
    class AllNeighboursPasser : INeighboursPasser
    {
        public void Pass()
        {
            foreach (StepPoint stepPoint in StepPointsList.AllPoints)
                NeighboursMarker.Mark(stepPoint);
        }
    }
}
