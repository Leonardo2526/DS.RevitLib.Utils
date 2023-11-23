using DS.ClassLib.VarUtils.Resolvers;
using DS.RevitCollisions.Models;

namespace DS.RevitCollisions
{
    /// <inheritdoc/>
    public abstract class MEPCollisionFactoryBuilderBase<TTask, TResult> :
        FactoryBuilderBase<IMEPCollision, TTask, TResult>
    { }
}
