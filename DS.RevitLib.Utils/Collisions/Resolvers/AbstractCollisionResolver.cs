using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Resolve;
using DS.RevitLib.Utils.Collisions.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    public abstract class AbstractCollisionResolver
    {
        protected AbstractCollisionResolver _successor;

        protected readonly ICollisionChecker _collisionChecker;

        protected AbstractCollisionResolver(ICollisionChecker collisionChecker)
        {
            _collisionChecker = collisionChecker;
        }

        public void SetSuccessor(AbstractCollisionResolver successor)
        {
            _successor = successor;
        }

        public bool IsResolved { get; protected set; }

        public abstract void Resolve();

    }
}
