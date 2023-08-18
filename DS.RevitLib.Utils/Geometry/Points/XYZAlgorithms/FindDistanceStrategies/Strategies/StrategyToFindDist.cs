using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Points.XYZAlgorithms.Strategies
{
    public abstract class StrategyToFindDist
    {
        public XYZ Point1 { get; protected set; }
        public XYZ Point2 { get; protected set; }

        public abstract double AlgorithmInterface(List<XYZ> points);
    }
}
