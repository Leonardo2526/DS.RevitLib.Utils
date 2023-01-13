using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Points.XYZAlgorithms.MaxDistance.Strategies
{
    public abstract class MaxDistnaceStrategy
    {
        public XYZ Point1 { get; protected set; }
        public XYZ Point2 { get; protected set; }

        public abstract double AlgorithmInterface(List<XYZ> points);
    }
}
