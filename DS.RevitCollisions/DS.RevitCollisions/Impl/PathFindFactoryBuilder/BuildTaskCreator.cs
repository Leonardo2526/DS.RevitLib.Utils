using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using Rhino.Geometry;
using System.Collections.Generic;

namespace DS.RevitCollisions.Impl
{
    /// <inheritdoc/>
    partial class PathFindFactoryBuilder : MEPCollisionFactoryBuilderBase<(IVertex, IVertex), List<Point3d>>
    {
        /// <inheritdoc/>
        public override ITaskCreator<IMEPCollision, (IVertex, IVertex)> BuildTaskCreator()
        {

            IEnumerator<(IVertex, IVertex)> itertor = null;
            var taskCreator = new VertexPairTaskCreator(itertor);


            return taskCreator;
        }

    }
}
