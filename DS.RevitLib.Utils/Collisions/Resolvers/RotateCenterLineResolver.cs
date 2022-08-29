using DS.RevitLib.Utils.Collisions.Checkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    internal class RotateCenterLineResolver : AbstractCollisionResolver
    {
        public RotateCenterLineResolver(ICollisionChecker collisionChecker) : base(collisionChecker)
        {
        }

        public override void Resolve()
        {
            throw new NotImplementedException();
        }
    }
}
