using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Path.CLZ;
using DS.RVT.ModelSpaceFragmentation.Points;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class PointsCloud
    {
        public PointsCloud(List<XYZ> sourcePoints)
        {
            SourcePoints = sourcePoints;
        }

        public List<XYZ> PointsCloudList { get; set; }

        public List<XYZ> SourcePoints { get; set; }

        public List<XYZ> GetXYZByCenterPoint(XYZ centerPoint, double distance)
        {
            PointsCloudList = new List<XYZ>();

            foreach (XYZ sourcePoint in SourcePoints)
            {
                if (centerPoint.DistanceTo(sourcePoint) <= distance)
                    PointsCloudList.Add(sourcePoint);
            }

            return PointsCloudList;
        }

        public List<StepPoint> GetStepPointByCenterPoint(XYZ centerPoint)
        {
            double angle = 45 / (180 / Math.PI);
            double distance = (CLZInfo.FullDistanceF + ModelSpacePointsGenerator.PointsStepF) / Math.Cos(angle);
            List<XYZ> unpassableXYZByCLZPoints = GetXYZByCenterPoint(centerPoint, distance);
            return PointsConvertor.XYZToStepPoints(unpassableXYZByCLZPoints);
        }
    }
}
