using DS.RevitLib.Utils.Collisions.Solutions;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    /// <summary>
    /// The interface used to create objects for collisions resoving.
    /// </summary>
    public interface IResolverAsync
    {
        /// <summary>
        /// Resolve collision asynchronously.
        /// </summary>
        /// <returns><see cref="Task{ISolution}"/> to get solution of collision.</returns>
        Task<ISolution> ResolveAsync();
    }
}
