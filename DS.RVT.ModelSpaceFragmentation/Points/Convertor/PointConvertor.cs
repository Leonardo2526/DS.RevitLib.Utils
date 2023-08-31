using Autodesk.Revit.DB;
using System;

namespace DS.RVT.ModelSpaceFragmentation.Points
{
    class PointConvertor
    {
        static readonly double PointsStepF = ModelSpacePointsGenerator.PointsStepF;


        public static XYZ StepPointToXYZ(StepPoint stepPoint)
        {
            XYZ refPoint = new XYZ(stepPoint.X * PointsStepF,
            stepPoint.Y * PointsStepF,
            stepPoint.Z * PointsStepF);

            return new XYZ(ElementInfo.MinBoundPoint.X + refPoint.X,
                ElementInfo.MinBoundPoint.Y + refPoint.Y,
                ElementInfo.MinBoundPoint.Z + refPoint.Z);

        }

        public static StepPoint XYZToStepPoint(XYZ point)
        {
            XYZ refPoint = new XYZ(point.X - ElementInfo.MinBoundPoint.X,
                point.Y - ElementInfo.MinBoundPoint.Y,
                point.Z - ElementInfo.MinBoundPoint.Z);

            return new StepPoint((int)Math.Round(refPoint.X / PointsStepF),
            (int)Math.Round(refPoint.Y / PointsStepF),
            (int)Math.Round(refPoint.Z / PointsStepF));

        }

        public static XYZ StepPointToXYZByPoint(XYZ basePoint, StepPoint stepPoint)
        {
            XYZ point = new XYZ(basePoint.X + stepPoint.X * PointsStepF,
                   basePoint.Y + stepPoint.Y * PointsStepF,
                   basePoint.Z + stepPoint.Z * PointsStepF);

            return point;
        }
    }
}
