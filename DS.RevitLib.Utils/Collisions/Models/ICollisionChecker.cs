using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.NewFolder1.ObjectsModel
{
    public  interface ICollisionChecker
    {
        public List<ICollision> GetCollisions();
    }
}
