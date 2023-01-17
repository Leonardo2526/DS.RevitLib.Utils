using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Collisions.Solutions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    /// <inheritdoc/>
    public abstract class CollisionResolverBase : ICollisionResolver
    {
        /// <summary>
        /// Instantiate an object to create resolver for collision .
        /// </summary>
        /// <param name="collision"></param>
        public CollisionResolverBase(ICollision collision)
        {

        }

        /// <inheritdoc/>
        public abstract ISolution Resolve();
    }
}
