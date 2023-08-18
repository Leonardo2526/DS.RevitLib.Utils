namespace DS.RVT.ModelSpaceFragmentation.Path.Neighbours
{
    class XZNeighboursPasser : INeighboursPasser
    {
        public void Pass()
        {
            foreach (StepPoint stepPoint in StepPointsList.XZPoints)
                NeighboursMarker.Mark(stepPoint);
        }
    }
}
