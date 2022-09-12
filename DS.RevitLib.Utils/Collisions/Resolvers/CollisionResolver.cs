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
        protected readonly List<ICollisionChecker> _collisionCheckers = new List<ICollisionChecker>();

        protected CollisionResolver(ICollision collision, ICollisionChecker collisionChecker)
        {
            Collision = collision;
            _collisionCheckers.Add(collisionChecker);
        }

        protected CollisionResolver(ICollision collision, List<ICollisionChecker> collisionChecker)
        {
            Collision = collision;
            _collisionCheckers = collisionChecker;
        }

        public ICollision Collision { get; }
        public bool IsResolved { get; protected set; }
        public List<ICollision> UnresolvedCollisions { get; protected set; }


        public abstract void Resolve();
        public void SetSuccessor(CollisionResolver successor)
        {
            _successor = successor;
        }

        protected List<ICollision> GetCollisions()
        {
            var collisions = new List<ICollision>();

            foreach (ICollisionChecker checker in _collisionCheckers)
            {
                var checkerCollisions = checker.GetCollisions();
                if (checkerCollisions != null && checkerCollisions.Any())
                {
                    collisions.AddRange(checkerCollisions);
                }
            }

            return collisions;
        }
    }
}
