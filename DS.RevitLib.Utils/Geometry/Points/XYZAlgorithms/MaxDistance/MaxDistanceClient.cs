using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Points.XYZAlgorithms.MaxDistance.Strategies;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Points.XYZAlgorithms.MaxDistance
{
    internal class MaxDistanceClient
    {
        private readonly List<XYZ> _points;
        private readonly MaxDistnaceStrategy _strategy;

        public MaxDistanceClient(List<XYZ> points, MaxDistnaceStrategy strategy)
        {
            _points = points;
            _strategy = strategy;
        }

        public XYZ Point1 { get; private set; }
        public XYZ Point2 { get; private set; }

        public double GetMaxDistance()
        {
            var max = _strategy.AlgorithmInterface(_points);
            Point1 = _strategy.Point1;
            Point2 = _strategy.Point2;

            return max;
        }
    }
}
