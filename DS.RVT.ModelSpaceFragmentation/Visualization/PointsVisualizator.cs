using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using DS.RVT.ModelSpaceFragmentation;

namespace DS.RVT.ModelSpaceFragmentation.Visualization
{
    class PointsVisualizator : IPointsVisualization
    {
        public bool OverwriteGraphic { get; set; }

        readonly List<XYZ> Points;
        public PointsVisualizator(List<XYZ> points)
        {
            Points = points;
        }

        public void Show(Document Doc)
        {
            VisiblePointsCreator visiblePointsCreator = new VisiblePointsCreator();
            visiblePointsCreator.Create(Doc, Points);

            if (OverwriteGraphic)
            {
                GraphicOverwriter graphicOverwriter = new GraphicOverwriter();
                Color color = new Color(255, 0, 0);
                graphicOverwriter.OverwriteElementsGraphic(visiblePointsCreator.Instances, color);
            }           
        }
    }
}
