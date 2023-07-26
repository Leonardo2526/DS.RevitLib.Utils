using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class IteratorByXZPlane : ISpacePointsIterator
    {
        public void Iterate()
        {
            int x, y, z;
            do
            {
                NeighboursPasser.A = 0;
                y = InputData.Ay;
                for (z = 0; z < InputData.Zcount; z++)
                {
                        for (x = 0; x < InputData.Xcount; x++)
                        {
                            if (!WaveExpander.Operation(x,y,z))
                                continue;
                        }
                }
            } while (!WaveExpander.Grid.ContainsKey(WaveExpander.EndStepPoint) && NeighboursPasser.A != 0);

        }
    }
}
