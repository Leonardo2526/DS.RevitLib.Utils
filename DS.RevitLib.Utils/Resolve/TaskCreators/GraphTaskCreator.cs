using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using DS.ClassLib.VarUtils.Resolvers;
using DS.ClassLib.VarUtils.Selectors;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.Resolve.TaskCreators;
using DS.RevitLib.Utils.Various;
using QuickGraph;
using Rhino.Geometry;
using System;

namespace DS.RevitLib.Utils.Resolve.TaskCreators
{
    /// <inheritdoc/>
    public class GraphTaskCreator<TItem> : TupleValidatableTaskCreator<TItem, IVertex>
    {
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
        private readonly Document _doc;

        /// <summary>
        /// Instansiate an object to create (<see cref="IVertex"/>, <see cref="IVertex"/>) tasks with <paramref name="selector"/>
        /// and specified <paramref name="graph"/>.
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="graph"></param>
        /// <param name="doc"></param>
        public GraphTaskCreator(IValidatableSelector<IVertex> selector,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, Document doc) :
            base(selector)
        {
            _graph = graph;
            _doc = doc;
        }

        /// <inheritdoc/>
        protected override IVertex Convert(IVertex item)
        {
            item = item is TaggedGVertex<(int, Point3d)> taggedIntPointVertex ?
            taggedIntPointVertex.ToVertexPoint(_graph.VertexCount) :
            item;

            item.TryFindTaggedVertex(_graph, out var foundVertex);
           
            return foundVertex ?? _graph.TryInsert(item, _doc);
        }
    }
}
