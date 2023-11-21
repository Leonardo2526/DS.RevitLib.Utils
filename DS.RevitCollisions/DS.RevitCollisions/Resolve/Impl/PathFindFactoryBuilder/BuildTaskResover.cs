using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace DS.RevitCollisions.Impl
{

    /// <inheritdoc/>
    partial class PathFindFactoryBuilder : MEPCollisionFactoryBuilderBase<(IVertex, IVertex), List<Point3d>>
    {
        /// <inheritdoc/>
        protected override ITaskResolver<(IVertex, IVertex), List<Point3d>> BuildTaskResover()
        {
            throw new NotImplementedException();
        }
    }
}
