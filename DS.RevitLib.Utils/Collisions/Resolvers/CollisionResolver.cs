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
    public abstract class CollisionResolver
    {
        protected CollisionResolver _successor;

        protected readonly ICollisionChecker _collisionChecker;
        protected readonly List<ICollisionChecker> _collisionCheckers;

        protected CollisionResolver(ICollision collision, ICollisionChecker collisionChecker)
        {
            Collision = collision;
            _collisionChecker = collisionChecker;
        }

        protected CollisionResolver(ICollision collision, List<ICollisionChecker> collisionChecker)
        {
            Collision = collision;
            _collisionCheckers = collisionChecker;
        }

        public ICollision Collision { get; }
        public bool IsResolved { get; protected set; }



        public abstract void Resolve();
        public void SetSuccessor(CollisionResolver successor)
        {
            _successor = successor;
        }

    }
}
