using DS.ClassLib.VarUtils.Resolvers;
using DS.RevitCollisions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions
{
    internal interface IMEPCollisionTaskCreator<TTask> : 
        ITaskCreator<IMEPCollision, TTask>
    { }
    
}
