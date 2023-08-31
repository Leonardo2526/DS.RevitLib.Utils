using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class IteratorByYZPlane : ISpacePointsIterator
    {
        public void Iterate()
        {
            int x, y, z;
            do
            {
                NeighboursPasser.A = 0;
                x = InputData.Ax;
                for (z = 0; z < InputData.Zcount; z++)
                {
                        for (y = 0; y < InputData.Ycount; y++)
                        {
                            if (!WaveExpander.Operation(x,y,z))
                                continue;
                        }
                }
            } while (!WaveExpander.Grid.ContainsKey(WaveExpander.EndStepPoint) && NeighboursPasser.A != 0);

        }
    }
}
