using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Points.XYZAlgorithms.Strategies
{
    public class StrategyToFindMinDist : StrategyToFindDist
    {
        public override double AlgorithmInterface(List<XYZ> points)
        {
            double min = Double.MaxValue;

            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    double dist = points[i].DistanceTo(points[j]);

                    if (dist < min)
                    {
                        min = dist;
                        Point1 = points[i];
                        Point2 = points[j];
                    }
                }
            }

            return min;
        }
    }
}
