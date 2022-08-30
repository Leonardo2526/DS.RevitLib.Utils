using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Collisions.Resolvers;
using DS.RevitLib.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.Collisions.Resolvers
{
    internal class RotateCenterLineResolver : CollisionResolver<TransformModel>
    {
        private readonly TransformModel _transformModel;

        public RotateCenterLineResolver(ICollision collision, ICollisionChecker collisionChecker, TransformModel transformModel) : base(collision, collisionChecker)
        {
            _transformModel = transformModel;
        }

        public override TransformModel Resolve()
        {
            throw new NotImplementedException();
        }
    }
}
