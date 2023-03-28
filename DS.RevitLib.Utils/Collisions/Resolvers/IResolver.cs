using DS.RevitLib.Utils.Collisions.Solutions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    /// <summary>
    /// The interface used to resolve collisions.
    /// </summary>
    public interface IResolver
    {
        /// <summary>
        /// Resolve collision.
        /// </summary>
        /// <returns>Returns solution for collision.</returns>
        ISolution Resolve();

        /// <summary>
        /// Resolve collision asynchronously.
        /// </summary>
        /// <returns>Returns solution for collision.</returns>
        Task<ISolution> ResolveAsync();

        /// <summary>
        /// All solutions for collision.
        /// </summary>
        List<ISolution> Solutions { get; }
    }
}
