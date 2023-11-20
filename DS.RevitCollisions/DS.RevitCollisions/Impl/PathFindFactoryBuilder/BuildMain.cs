using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using Rhino.Geometry;
using System.Collections.Generic;

namespace DS.RevitCollisions.Impl
{

    /// <inheritdoc/>
    partial class PathFindFactoryBuilder : MEPCollisionFactoryBuilderBase<(IVertex, IVertex), List<Point3d>>
    {
       
        public PathFindFactoryBuilder()
        {
          
        }


        public IMEPCollision Collision { get; set; }

        public PathFindFactoryBuilder WithCollision(IMEPCollision mEPCollision)
        {
            Collision = mEPCollision;
            return this;
        }

        private string _name;
    }
}
