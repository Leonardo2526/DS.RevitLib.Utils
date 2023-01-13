using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Points.XYZAlgorithms.MaxDistance.Strategies
{
    public class NaiveStrategy : MaxDistnaceStrategy
    {
        public override double AlgorithmInterface(List<XYZ> points)
        {
            double max = 0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    double dist = points[i].DistanceTo(points[j]);

                    if (dist > max)
                    {
                        max = dist;
                        Point1 = points[i];
                        Point2 = points[j];
                    }
                }
            }

            return max;
        }
    }
}
