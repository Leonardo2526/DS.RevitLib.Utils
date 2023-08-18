using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RVT.ModelSpaceFragmentation.Visualization;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class SpaceFragmentator
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public SpaceFragmentator(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }

        public static List<XYZ> PassablePoints { get; set; }
        public static List<XYZ> UnpassablePoints { get; set; }

        public void FragmentSpace(Element element)
        {

            ElementInfo pointsInfo = null;
            pointsInfo.GetPoints();

            ModelSpacePointsGenerator modelSpacePointsGenerator =
            new ModelSpacePointsGenerator(ElementInfo.MinBoundPoint, ElementInfo.MaxBoundPoint);
            List<XYZ> spacePoints = modelSpacePointsGenerator.Generate();


            int c1 = SpaceZone.ZoneCountX;
            double s1 = SpaceZone.ZoneSizeX;

            List<BoundingBoxXYZ> boundingBoxes = BoundingBoxCreator.Create();

            //Get list with BoundingBoxIntersectsFilters
            List<BoundingBoxIntersectsFilter> bbFilters = new List<BoundingBoxIntersectsFilter>();
            foreach (BoundingBoxXYZ bb in boundingBoxes)
            {
                Outline myOutLn = new Outline(bb.Min, bb.Max);

                bbFilters.Add(new BoundingBoxIntersectsFilter(myOutLn));

                //Show boundingBoxes
                //BoundigBoxVizualizator.ShowBoudaries(bb);
            }

            ModelSolid modelSolid = new ModelSolid(Doc);

            //Get Outlines with solids
            Dictionary<Outline, List<Solid>> outlinesSolids = new Dictionary<Outline, List<Solid>>();
            foreach (BoundingBoxIntersectsFilter bbf in bbFilters)
            {
                List<Solid> bbSolids = modelSolid.GetSolidsByBBF(bbf);
                if (bbSolids.Count > 0)
                    outlinesSolids.Add(bbf.GetBoundingBox(), bbSolids);

            }

            //foreach (Outline outline in outlines)
            //{
            //    BoundingBoxXYZ boundingBoxXYZ = new BoundingBoxXYZ();
            //    boundingBoxXYZ.Min = outline.MinimumPoint;
            //    boundingBoxXYZ.Max = outline.MaximumPoint;
            //   BoundigBoxVizualizator.ShowBoudaries(boundingBoxXYZ);
            //}


            PointsSeparator pointsSeparator = new PointsSeparator(spacePoints, outlinesSolids);
            pointsSeparator.Separate();

            UnpassablePoints = pointsSeparator.UnpassablePoints;
            PassablePoints = pointsSeparator.PassablePoints;

            //Visualize(pointsSeparator);
        }

        private void Visualize(PointsSeparator pointsSeparator)
        {
            Visualizator.ShowPoints(new PointsVisualizator(pointsSeparator.PassablePoints));

            IPointsVisualization unpassablePointsVisualization = new PointsVisualizator(pointsSeparator.UnpassablePoints)
            {
                OverwriteGraphic = true
            };
            Visualizator.ShowPoints(unpassablePointsVisualization);
        }
    }
}
