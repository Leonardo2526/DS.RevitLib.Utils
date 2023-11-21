using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Resolvers;

namespace DS.RevitCollisions
{
    /// <inheritdoc/>
    internal class CollisionResolveFactory<TTask, TResult> : ResolveFactory<ICollision, TTask, TResult>
    {
        /// <inheritdoc/>
        public CollisionResolveFactory(ITaskCreator<ICollision, TTask> taskCreator,
            ITaskResolver<TTask, TResult> taskResolver) :
            base(taskCreator, taskResolver)
        {
        }
    }
}
