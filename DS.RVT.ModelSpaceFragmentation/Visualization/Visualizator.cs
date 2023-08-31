using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using DS.RVT.ModelSpaceFragmentation;

namespace DS.RVT.ModelSpaceFragmentation.Visualization
{
    class Visualizator
    {
        public static void ShowPoints(IPointsVisualization pointsVisualization)
        {
            pointsVisualization.Show(Main.Doc);
        }
    }
}
