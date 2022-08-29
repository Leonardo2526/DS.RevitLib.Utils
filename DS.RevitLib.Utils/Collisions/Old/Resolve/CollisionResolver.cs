using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolve
{
    public abstract class CollisionResolver
    {
        protected ICollisionSearch _collisionSearch;

        protected CollisionResolver(ICollisionSearch collisionSearch)
        {
            _collisionSearch = collisionSearch;
        }

        protected CollisionResolver _successor;

        public void SetSuccessor (CollisionResolver successor)
        {
            _successor = successor;
        }

        public bool IsResolved { get; protected set; }

        public abstract void Resolve();
    }
}
