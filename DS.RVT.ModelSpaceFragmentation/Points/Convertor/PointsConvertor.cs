using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Points
{
    class PointsConvertor
    {
        public static List<XYZ> StepPointsToXYZ(List<StepPoint> stepPoints)
        {
            List<XYZ> XYZpoints = new List<XYZ>();
            
            foreach (StepPoint stepPoint in stepPoints)
                XYZpoints.Add(PointConvertor.StepPointToXYZ(stepPoint));

            return XYZpoints;
        }

        public static List<StepPoint> XYZToStepPoints(List<XYZ> XYZPoints)
        {
            List<StepPoint> stepPoints = new List<StepPoint>();

            if (XYZPoints == null)
                return stepPoints;

            foreach (XYZ XYZPoint in XYZPoints)
                stepPoints.Add(PointConvertor.XYZToStepPoint(XYZPoint));

            return stepPoints;
        }

        public static List<XYZ> StepPointsToXYZByBasePoint(XYZ basePoint, List<StepPoint> CLZPoints)
        {
            List<XYZ> modelSpacePoints = new List<XYZ>();

            foreach (StepPoint stepPoint in CLZPoints)
                modelSpacePoints.Add(PointConvertor.StepPointToXYZByPoint(basePoint, stepPoint));

            return modelSpacePoints;
        }

    }
}
