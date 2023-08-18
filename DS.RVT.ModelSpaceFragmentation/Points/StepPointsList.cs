using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation
{
    class StepPointsList
    {
        public static List<StepPoint> XPoints { get; } = new List<StepPoint>()
            {
                 new StepPoint(-1, 0, 0),
                new StepPoint(1, 0, 0)
            };

        public static List<StepPoint> YPoints { get; } = new List<StepPoint>()
            {
                 new StepPoint(0, -1, 0),
                new StepPoint(0, 1, 0),
            };

        public static List<StepPoint> ZPoints { get;} = new List<StepPoint>()
            {
                  new StepPoint(0, 0, -1),
                new StepPoint(0, 0, 1)
            };

        private static List<StepPoint> stepPoints;
        public static List<StepPoint> XYPoints
        {
            get
            {
                stepPoints = new List<StepPoint>();
                stepPoints.AddRange(XPoints);
                stepPoints.AddRange(YPoints);
                return stepPoints;
            }

        }
        public static List<StepPoint> XZPoints
        {
            get
            {
                stepPoints = new List<StepPoint>();
                stepPoints.AddRange(XPoints);
                stepPoints.AddRange(ZPoints);
                return stepPoints;
            }

        }
        public static List<StepPoint> YZPoints
        {
            get
            {
                stepPoints = new List<StepPoint>();
                stepPoints.AddRange(YPoints);
                stepPoints.AddRange(ZPoints);
                return stepPoints;
            }

        }
        public static List<StepPoint> AllPoints
        {
            get
            {
                stepPoints = new List<StepPoint>();
                stepPoints.AddRange(XPoints);
                stepPoints.AddRange(YPoints);
                stepPoints.AddRange(ZPoints);
                return stepPoints;
            }

        }
    }
}
