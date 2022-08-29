using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Models
{
    public abstract class AbstractCollision<T, P> : ICollision
    {
        public T BaseObject { get; }
        public List<P> CollisionObjects { get; }

        public AbstractCollision(T baseObject, List<P> collisionObjects)
        {
            BaseObject = baseObject;
            CollisionObjects = collisionObjects;
        }

        public abstract object GetIntersection();
    }
}
