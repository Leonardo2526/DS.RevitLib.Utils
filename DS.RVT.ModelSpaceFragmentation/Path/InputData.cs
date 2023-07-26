using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{
    class InputData
    {
        /// <summary>
        /// Path coordinates by x
        /// </summary>
        public static int[] Px { get; set; }
        /// <summary>
        /// Path coordinates by y
        /// </summary>
        public static int[] Py { get; set; }

        /// <summary>
        /// Start point coordinate X in reference coordinates
        /// </summary>
        public static int Ax { get; set; }

        /// <summary>
        /// Start point coordinate Y in reference coordinates
        /// </summary>
        public static int Ay { get; set; }

        /// <summary>
        /// Start point coordinate Z in reference coordinates
        /// </summary>
        public static int Az { get; set; }

        /// <summary>
        /// End point coordinate X in reference coordinates
        /// </summary>
        public static int Bx { get; set; }

        /// <summary>
        /// End point coordinate X in reference coordinates
        /// </summary>
        public static int By { get; set; }

        /// <summary>
        /// End point coordinate Z in reference coordinates
        /// </summary>
        public static int Bz { get; set; }

        public static int Xcount { get; set; }
        public static int Ycount { get; set; }
        public static int Zcount { get; set; }
        public static List<int> UnpassLocX { get; set; }
        public static List<int> UnpassLocY { get; set; } 
        public static List<int> UnpassLocZ { get; set; }
        public static List<StepPoint> UnpassStepPoints { get; set; }
        public static double PointsStepF { get; set; }
        public static XYZ ZonePoint1 { get; set; }
        public static XYZ ZonePoint2 { get; set; }

        public XYZ StartPoint { get; set; }
        public XYZ EndPoint { get; set; }
        public List<XYZ> UnpassablePoints { get; set; } = new List<XYZ>();   

        public InputData (XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            UnpassablePoints = unpassablePoints;
        }

        public void ConvertToPlane()
        {
            ZonePoint1 = ElementInfo.MinBoundPoint;
            ZonePoint2 = ElementInfo.MaxBoundPoint;
            PointsStepF = ModelSpacePointsGenerator.PointsStepF;

            UnpassLocX = new List<int>();
            UnpassLocY = new List<int>();
            UnpassLocZ = new List<int>();

            double axdbl = (StartPoint.X - ZonePoint1.X) / PointsStepF;
            double aydbl = (StartPoint.Y - ZonePoint1.Y) / PointsStepF;
            double azdbl = (StartPoint.Z - ZonePoint1.Z) / PointsStepF;
            double bxdbl = (EndPoint.X - ZonePoint1.X) / PointsStepF;
            double bydbl = (EndPoint.Y - ZonePoint1.Y) / PointsStepF;
            double bzdbl = (EndPoint.Z - ZonePoint1.Z) / PointsStepF;

            Ax = (int)Math.Round(axdbl);
            Ay = (int)Math.Round(aydbl);
            Az = (int)Math.Round(azdbl);

            Bx = (int)Math.Round(bxdbl);
            By = (int)Math.Round(bydbl);
            Bz = (int)Math.Round(bzdbl);

            Xcount = ModelSpacePointsGenerator.Xcount;
            Ycount = ModelSpacePointsGenerator.Ycount;
            Zcount = ModelSpacePointsGenerator.Zcount;

            UnpassStepPoints = new List<StepPoint>();
            if (UnpassablePoints.Count != 0)
            {
                foreach (XYZ point in UnpassablePoints)
                {
                    int X = (int)Math.Round((point.X - ZonePoint1.X) / PointsStepF);
                    int Y = (int)Math.Round((point.Y - ZonePoint1.Y) / PointsStepF);
                    int Z = (int)Math.Round((point.Z - ZonePoint1.Z) / PointsStepF);
                    UnpassLocX.Add(X);
                    UnpassLocY.Add(Y);
                    UnpassLocZ.Add(Z);
                    StepPoint stepPoint = new StepPoint(X, Y, Z);
                    UnpassStepPoints.Add(stepPoint);
                }
            }

          
        }
    }
}
