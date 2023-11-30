using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions.Resolve.TaskCreators
{
    internal class DummyTaskCreator : ITaskCreator<IMEPCollision, string>
    {
        public string CreateTask(IMEPCollision item)
        {
            return "";
        }
    }
}
