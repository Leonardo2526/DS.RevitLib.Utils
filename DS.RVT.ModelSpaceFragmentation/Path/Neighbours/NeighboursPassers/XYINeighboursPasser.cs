namespace DS.RVT.ModelSpaceFragmentation.Path.Neighbours
{
    class XYINeighboursPasser : INeighboursPasser
    {
        public void Pass()
        {
            foreach (StepPoint stepPoint in StepPointsList.XYPoints)
                NeighboursMarker.Mark(stepPoint);
        }
    }
}
