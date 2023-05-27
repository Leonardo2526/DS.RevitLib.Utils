using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.Collisions.Solutions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    /// <summary>
    /// The interface used to create objects for collisions resoving.
    /// </summary>
    public interface IResolver
    {
        /// <summary>
        /// Resolve collision.
        /// </summary>
        /// <param name="collision">Collision to resolve</param>
        /// <returns>Solution of <paramref name="collision"/>.</returns>
        ISolution Resolve(ICollision collision);

        /// <summary>
        /// Resolve collision asynchronously.
        /// </summary>
        /// <param name="collision">Collision to resolve</param>
        /// <returns><see cref="Task{ISolution}"/> to get solution of <paramref name="collision"/>.</returns>
        Task<ISolution> ResolveAsync(ICollision collision);
    }

    /// <summary>
    /// The interface used to create objects for collisions resoving.
    /// </summary>
    public interface IResolver<TSolution, TCollision>
        where TSolution : ISolution 
        where TCollision : ICollision
    {
        /// <summary>
        /// Resolve collision.
        /// </summary>
        /// <param name="collision">Collision to resolve</param>
        /// <returns>Solution of <paramref name="collision"/>.</returns>
        TSolution Resolve(TCollision collision);

        /// <summary>
        /// Resolve collision asynchronously.
        /// </summary>
        /// <param name="collision">Collision to resolve</param>
        /// <returns><see cref="Task{TSolution}"/> to get solution of <paramref name="collision"/>.</returns>
        Task<TSolution> ResolveAsync(TCollision collision);
    }

    /// <summary>
    /// The interface used to create objects for collisions resoving.
    /// </summary>
    public interface IResolver<TCollision>
        where TCollision : ICollision
    {
        /// <summary>
        /// Resolve collision.
        /// </summary>
        /// <param name="collision">Collision to resolve</param>
        /// <returns>Solutions of <paramref name="collision"/>.</returns>
        List<ISolution> Resolve(TCollision collision);
    }

    /// <summary>
    /// The interface used to create objects for asynchronously collisions resoving.
    /// </summary>
    public interface IMultiResolverAsync<TCollision>
        where TCollision : ICollision
    {     
        /// <summary>
        /// Resolve collision asynchronously.
        /// </summary>
        /// <param name="collision">Collision to resolve</param>
        /// <returns><see cref="Task{TSolution}"/> to get solutions of <paramref name="collision"/>.</returns>
        Task<List<ISolution>> ResolveAsync(TCollision collision);
    }

    /// <summary>
    /// The interface used to create objects for asynchronously collisions resoving.
    /// </summary>
    public interface IResolverAsync<TCollision>
        where TCollision : ICollision
    {
        /// <summary>
        /// Resolve collision asynchronously.
        /// </summary>
        /// <param name="collision">Collision to resolve</param>
        /// <returns><see cref="Task{TSolution}"/> to get solutions of <paramref name="collision"/>.</returns>
        Task<ISolution> ResolveAsync(TCollision collision);
    }


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
