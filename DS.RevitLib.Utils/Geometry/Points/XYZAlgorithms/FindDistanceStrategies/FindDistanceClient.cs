using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Points.XYZAlgorithms.Strategies;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Points.XYZAlgorithms.MaxDistance
{
    internal class FindDistanceClient
    {
        private readonly List<XYZ> _points;
        private readonly StrategyToFindDist _strategy;

        public FindDistanceClient(List<XYZ> points, StrategyToFindDist strategy)
        {
            _points = points;
            _strategy = strategy;
        }

        public XYZ Point1 { get; private set; }
        public XYZ Point2 { get; private set; }

        public double GetDistance()
        {
            var dist = _strategy.AlgorithmInterface(_points);
            Point1 = _strategy.Point1;
            Point2 = _strategy.Point2;

            return dist;
        }
    }
}
