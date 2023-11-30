using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Resolvers;
using System.Collections.Generic;

namespace DS.RevitCollisions.Resolve.ResolveProcessors
{
    /// <inheritdoc/>
    internal class CollisionResolveProcessor<TResult> : ResolveProcessor<ICollision, TResult>
    {
        /// <inheritdoc/>
        public CollisionResolveProcessor(IEnumerable<IResolveFactory<ICollision, TResult>> resolveFactories) :
            base(resolveFactories)
        {
        }
    }
}
