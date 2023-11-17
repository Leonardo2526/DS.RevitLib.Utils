using DS.ClassLib.VarUtils.Resolvers.ResolveTasks;
using DS.GraphUtils.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions
{
    public class PathFindVertexTask : PathFindTask<IVertex>
    {
        public PathFindVertexTask(IVertex source, IVertex target) : 
            base(source, target)
        {
        }
    }
}
