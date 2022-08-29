using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    public abstract class CollisionResolver<T>
    {
        protected CollisionResolver<T> _successor;

        protected readonly ICollisionChecker _collisionChecker;

        protected CollisionResolver(ICollision collision, ICollisionChecker collisionChecker)
        {
            Collision = collision;
            _collisionChecker = collisionChecker;
        }

        public ICollision Collision { get; }

        public T Solution { get; protected set; }


        public abstract T Resolve();
        public void SetSuccessor(CollisionResolver<T> successor)
        {
            _successor = successor;
        }

    }
}
