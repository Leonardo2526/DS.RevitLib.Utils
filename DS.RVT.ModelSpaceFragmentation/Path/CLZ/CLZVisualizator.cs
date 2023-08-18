using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Points;
using DS.RVT.ModelSpaceFragmentation.Visualization;
using DS.RVT.ModelSpaceFragmentation.Path.CLZ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path.CLZ
{
    class CLZVisualizator
    {
        public static void ShowCLZOfPoint(XYZ basePoint)
        {
            List<XYZ> CLZPoints = PointsConvertor.StepPointsToXYZByBasePoint(basePoint, CLZInfo.Points);
            Visualizator.ShowPoints(new PointsVisualizator(CLZPoints));
        }
    }
}
