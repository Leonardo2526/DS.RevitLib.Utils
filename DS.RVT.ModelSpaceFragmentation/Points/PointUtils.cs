using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DS.RVT.ModelSpaceFragmentation
{
    class PointUtils
    {
        public void FindMinMaxPointByPoints(List<XYZ> points, out XYZ minPoint, out XYZ maxPoint)
        {
            List<double> xlist = new List<double>();
            List<double> ylist = new List<double>();
            List<double> zlist = new List<double>();

            foreach (XYZ point in points)
            {
                xlist.Add(point.X);
                ylist.Add(point.Y);
                zlist.Add(point.Z);
            }

            minPoint = new XYZ(xlist.Min(a => a), ylist.Min(a => a), zlist.Min(a => a));
            maxPoint = new XYZ(xlist.OrderByDescending(a => a).First(),
                ylist.OrderByDescending(a => a).First(), zlist.OrderByDescending(a => a).First());
        }

        public void FindMinMaxPointByLines(List<Line> allCurrentPositionLines, out XYZ minPoint, out XYZ maxPoint)
        {
            List<XYZ> points = new List<XYZ>();

            foreach (Line line in allCurrentPositionLines)
            {
                points.Add(line.GetEndPoint(0));
                points.Add(line.GetEndPoint(1));
            }

            FindMinMaxPointByPoints(points, out minPoint, out maxPoint);
        }

    }    
}
