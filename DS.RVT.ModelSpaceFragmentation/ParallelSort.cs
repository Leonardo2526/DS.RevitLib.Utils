using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation
{
    class ParallelSort
    {
        private readonly Dictionary<Outline, List<Solid>> OutlinesWithSolids;
        private readonly List<XYZ> PointsInOutlines;

        public Dictionary<Outline, List<XYZ>> SortedPoints  = new Dictionary<Outline, List<XYZ>>();

        public ParallelSort(Dictionary<Outline, List<Solid>> outlinesWithSolids, List<XYZ> pointsInOutlines)
        {
            OutlinesWithSolids = outlinesWithSolids;
            PointsInOutlines = pointsInOutlines;
        }

        private void CheckPoints(int n)
        {
            
                List<XYZ> pointsInOutline = new List<XYZ>();

                List<int> indexes = new List<int>();
                for (int i = 0; i < PointsInOutlines.Count; i++)
                {
                    if (OutlinesWithSolids.ElementAt(n).Key.Contains(PointsInOutlines[i], 0))
                    {
                        indexes.Add(i);
                        pointsInOutline.Add(PointsInOutlines[i]);
                    }
                }

                //for (int i = indexes.Count - 1; i-- > 0;)
                //    pointsInAllOutlines.RemoveAt(i);

                SortedPoints.Add(OutlinesWithSolids.ElementAt(n).Key, pointsInOutline);
           
        }


        public void RunSort()
        {
            Parallel.For(0, OutlinesWithSolids.Count, CheckPoints);
        }

    }
}
