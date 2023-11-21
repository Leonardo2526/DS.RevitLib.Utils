using Autodesk.Revit.DB;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Models;
using QuickGraph;
using Rhino.Geometry;
using System.Collections.Generic;

namespace DS.RevitCollisions.Impl
{

    /// <inheritdoc/>
    partial class PathFindFactoryBuilder : MEPCollisionFactoryBuilderBase<(IVertex, IVertex), PointsList>
    {
        private string _name;
        private readonly Document _doc;
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
       
        public PathFindFactoryBuilder(Document doc, IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            _doc = doc;
            _graph = graph;
        }

        public Dictionary<BuiltInCategory, List<PartType>> IterationCategories { get; set; }

        public IMEPCollision Collision { get; set; }

        public PathFindFactoryBuilder WithCollision(IMEPCollision mEPCollision)
        {
            Collision = mEPCollision;
            return this;
        }

    }
}
