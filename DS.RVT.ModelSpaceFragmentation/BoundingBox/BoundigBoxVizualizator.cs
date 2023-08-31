using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Lines;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    static class BoundigBoxVizualizator
    {
        public static void ShowBoudaries(BoundingBoxXYZ bb)
        {
            LineCreator lineCreator = new LineCreator();

            List<XYZ> bbBottomPoints = new List<XYZ>()
            {
                //bottom
                bb.Min,
                new XYZ (bb.Min.X,bb.Max.Y, bb.Min.Z),
                new XYZ (bb.Max.X,bb.Max.Y, bb.Min.Z),
                new XYZ (bb.Max.X,bb.Min.Y, bb.Min.Z),
                bb.Min
            };
            lineCreator.CreateCurves(new CurvesByPointsCreator(bbBottomPoints));

            List<XYZ> bbTopPoints = new List<XYZ>()
            {
                bb.Max,
                new XYZ (bb.Max.X,bb.Min.Y, bb.Max.Z),
                new XYZ (bb.Min.X,bb.Min.Y, bb.Max.Z),
                new XYZ (bb.Min.X,bb.Max.Y, bb.Max.Z),
                bb.Max
            };
            lineCreator.CreateCurves(new CurvesByPointsCreator(bbTopPoints));


            List<XYZ> bbSidePoints = new List<XYZ>()
            {
                bb.Min,
                new XYZ (bb.Min.X,bb.Min.Y, bb.Max.Z),
            };
            lineCreator.CreateCurves(new CurvesByPointsCreator(bbSidePoints));

            bbSidePoints = new List<XYZ>()
            {
                 new XYZ (bb.Min.X,bb.Max.Y, bb.Min.Z),
                new XYZ (bb.Min.X,bb.Max.Y, bb.Max.Z),
            };
            lineCreator.CreateCurves(new CurvesByPointsCreator(bbSidePoints));

            bbSidePoints = new List<XYZ>()
            {
                new XYZ (bb.Max.X,bb.Max.Y, bb.Min.Z),
                new XYZ (bb.Max.X,bb.Max.Y, bb.Max.Z)
            };
            lineCreator.CreateCurves(new CurvesByPointsCreator(bbSidePoints));

            bbSidePoints = new List<XYZ>()
            {
                new XYZ (bb.Max.X,bb.Min.Y, bb.Min.Z),
                new XYZ (bb.Max.X,bb.Min.Y, bb.Max.Z)
            };
            lineCreator.CreateCurves(new CurvesByPointsCreator(bbSidePoints));


        }
    }
}
