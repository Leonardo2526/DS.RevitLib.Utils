using Autodesk.Revit.DB;
using DS.RVT.ModelSpaceFragmentation.Points;
using DS.RVT.ModelSpaceFragmentation.Visualization;
using DS.RVT.ModelSpaceFragmentation.Path.CLZ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class CLZCretor
    {
        public List<StepPoint> CLZPoints { get; set; }

        public List<StepPoint> Create(IZonePoints zonePoints)
        {
            CLZInfo.GetInfo();

            CLZPoints = zonePoints.Create();
            CLZInfo.Points = CLZPoints;
            return CLZPoints;
        }
    }
}
