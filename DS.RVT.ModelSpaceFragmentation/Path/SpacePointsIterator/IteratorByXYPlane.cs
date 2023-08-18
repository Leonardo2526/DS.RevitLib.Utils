using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class IteratorByXYPlane : ISpacePointsIterator
    {
        public void Iterate()
        {
            int x, y, z;
            do
            {
                NeighboursPasser.A = 0;
                z = InputData.Az;
                    for (y = 0; y < InputData.Ycount; y++)
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
