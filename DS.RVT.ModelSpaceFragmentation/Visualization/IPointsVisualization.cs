using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation.Visualization
{
    interface IPointsVisualization
    {
        public bool OverwriteGraphic { get; set; }

        void Show(Document Doc);
    }
}
