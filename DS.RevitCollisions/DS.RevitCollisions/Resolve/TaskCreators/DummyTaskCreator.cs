using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions.TaskCreators
{
    internal class DummyTaskCreator : IMEPCollisionTaskCreator<string>
    {
        public string CreateTask(IMEPCollision item)
        {
            return "";
        }
    }
}
