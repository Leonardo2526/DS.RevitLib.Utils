using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path.Neighbours
{ 
    class NeighboursMarker
    {
        public static void Mark(StepPoint stepPoint)
        {
            int ix = NeighboursPasser.CurrentPoint.X + stepPoint.X,
            iy = NeighboursPasser.CurrentPoint.Y + stepPoint.Y,
            iz = NeighboursPasser.CurrentPoint.Z + stepPoint.Z;

            if (ix >= 0 && ix < InputData.Xcount &&
           iy >= 0 && iy < InputData.Ycount &&
           iz >= 0 && iz < InputData.Zcount)
            {
                StepPoint nextPoint = new StepPoint(ix, iy, iz);

                GridPointChecker gridPointChecker =
                    new GridPointChecker(nextPoint);
                gridPointChecker.Check();
            }
        }
    }
}
