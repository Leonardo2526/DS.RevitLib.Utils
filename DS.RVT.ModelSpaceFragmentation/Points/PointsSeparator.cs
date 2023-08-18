using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RVT.ModelSpaceFragmentation.Lines;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Extensions;

namespace DS.RVT.ModelSpaceFragmentation
{
    class PointsSeparator
    {
        List<XYZ> SpacePoints;
        Dictionary<Outline, List<Solid>> OutlinesWithSolids;

        public PointsSeparator(List<XYZ> spacePoints, Dictionary<Outline, List<Solid>> outlinesSolids)
        {
            SpacePoints = spacePoints;
            OutlinesWithSolids = outlinesSolids;
        }

        public List<XYZ> PassablePoints { get; set; } = new List<XYZ>();
        public List<XYZ> UnpassablePoints { get; set; } = new List<XYZ>();

        public void Separate()
        {

            Dictionary<Outline, List<XYZ>> sortedPoints = SortPointsByOutlinesTPL();


            //Separate points inside each outline 
            LineCreator lineCreator = new LineCreator();
            SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();
            LineCollision lineCollision = new LineCollision(intersectOptions);
            PointInSolidChecker pointInSolidChecker = new PointInSolidChecker(lineCreator, lineCollision);

            foreach (KeyValuePair<Outline, List<XYZ>> keyValue in sortedPoints)
            {
                OutlinesWithSolids.TryGetValue(keyValue.Key, out List<Solid> solids);

                List<XYZ> pointsForSearch = GetPointsInSolids(keyValue.Value, solids);

                foreach (XYZ point in pointsForSearch)
                {                    
                    if (pointInSolidChecker.IsPointInSolid(point, solids))
                        UnpassablePoints.Add(point);
                    else
                    { PassablePoints.Add(point); }
                }
            }

            //For visualization only
            //foreach (XYZ point in SpacePoints)
            //{
            //    if (!UnpassablePoints.Contains(point))
            //        PassablePoints.Add(point);
            //}

        }

        private Dictionary<Outline, List<XYZ>> SortPointsByOutlinesTPL()
        {

            List<XYZ> pointsInOutlines = GetPointsInOutlines();

            ParallelSort parallelSort = new ParallelSort(OutlinesWithSolids, pointsInOutlines);
            parallelSort.RunSort();
            Dictionary<Outline, List<XYZ>> sortedPoints = parallelSort.SortedPoints;

            return sortedPoints;

        }

        private List<XYZ> GetPointsInOutlines()
        {
            List<XYZ> xYZs = new List<XYZ>();

            XYZ minPoint = OutlinesWithSolids.First().Key.MinimumPoint;
            XYZ maxPoint = OutlinesWithSolids.Last().Key.MaximumPoint;

            Outline outline = new Outline(minPoint, maxPoint);

            for (int i = 0; i < SpacePoints.Count; i++)
            {
                if (outline.Contains(SpacePoints[i], 0))
                    xYZs.Add(SpacePoints[i]);
            }

            return xYZs;
        }

        private List<XYZ> GetPointsInSolids(List<XYZ> points , List<Solid> solids)
        {
            List<XYZ> pointsForSearch = new List<XYZ>();

            foreach (Solid solid in solids)
            {
                Transform transform = solid.GetBoundingBox().Transform;
                Solid solidTransformed = SolidUtils.CreateTransformed(solid, transform);

                XYZ minPoint = solidTransformed.GetBoundingBox().Min;
                XYZ maxPoint = solidTransformed.GetBoundingBox().Max;

                XYZ minTrPoint = transform.OfPoint(minPoint);
                XYZ maxTrPoint = transform.OfPoint(maxPoint);

                Outline outline = new Outline(minTrPoint, maxTrPoint);

                foreach (XYZ point in points)
                {
                    //if (point == null)
                    //{
                    //    TaskDialog.Show("Revit", "null Point");
                    //}
                    if (point != null && outline.Contains(point, 0))
                        pointsForSearch.Add(point);
                }
            }

            return pointsForSearch;
        }

    }
}
