using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Collisions.Solutions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    /// <summary>
    /// The interface used to create resolver for collision .
    /// </summary>
    internal interface ICollisionResolver
    {
        /// <summary>
        /// Resolve collision.
        /// </summary>
        /// <returns>Returns collision's solution.</returns>
        ISolution Resolve();
    }

}
