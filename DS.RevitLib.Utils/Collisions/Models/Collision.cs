using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Models
{
    public abstract class Collision<T, P> : ICollision
    {
        public T Object1 { get; }
        public P Object2 { get; }

        public Collision(T object1, P object2)
        {
            Object1 = object1;
            Object2 = object2;
        }
    }
}
