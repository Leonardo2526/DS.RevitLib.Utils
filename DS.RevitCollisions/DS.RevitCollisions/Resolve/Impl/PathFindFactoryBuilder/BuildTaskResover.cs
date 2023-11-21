using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace DS.RevitCollisions.Impl
{

    /// <inheritdoc/>
    partial class PathFindFactoryBuilder : MEPCollisionFactoryBuilderBase<(IVertex, IVertex), PointsList>
    {
        /// <inheritdoc/>
        protected override ITaskResolver<(IVertex, IVertex), PointsList> BuildTaskResover()
        {
            var resolver = new PathFindVertexPairResolver();
            return resolver;
        }
    }
}
