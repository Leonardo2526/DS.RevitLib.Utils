using DS.RevitLib.Utils.Collisions.Models;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Collisions.Checkers
{
    public interface ICollisionChecker
    {
        public List<ICollision> GetCollisions();
    }
}
