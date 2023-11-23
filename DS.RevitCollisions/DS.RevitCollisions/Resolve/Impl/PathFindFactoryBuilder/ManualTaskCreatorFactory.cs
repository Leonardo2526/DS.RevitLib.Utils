using DS.ClassLib.VarUtils.Resolvers;
using DS.ClassLib.VarUtils.Resolvers.TaskCreators;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions.Resolve.Impl.PathFindFactoryBuilder
{
    internal class ManualTaskCreatorFactory : ITaskCreatorFactory<IMEPCollision, (IVertex, IVertex)>
    {
        public ITaskCreator<IMEPCollision, (IVertex, IVertex)> Create()
        {
            throw new NotImplementedException();
        }
    }
}
