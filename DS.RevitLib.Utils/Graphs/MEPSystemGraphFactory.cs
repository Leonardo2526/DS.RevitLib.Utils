using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using MoreLinq.Extensions;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;

namespace DS.RevitLib.Utils.Graphs
{
    /// <inheritdoc/>
    public class MEPSystemGraphFactory : MEPSystemGraphFactoryBase<AdjacencyGraph<IVertex, Edge<IVertex>>>
    {
        private readonly IVertexBuilder<IVertex, TaggedGVertex<int>> _vertexBuilder;
        private readonly IEdgeBuilder<IVertex, Edge<IVertex>> _edgeBuilder;

        /// <inheritdoc/>
        public MEPSystemGraphFactory(Document doc,
            IVertexBuilder<IVertex, TaggedGVertex<int>> vertexBuilder,
            IEdgeBuilder<IVertex, Edge<IVertex>> edgeBuilder) : base(doc)
        {
            _vertexBuilder = vertexBuilder;
            _edgeBuilder = edgeBuilder;
            _graph = new AdjacencyGraph<IVertex, Edge<IVertex>>();
        }

        /// <summary>
        /// 
        /// </summary>
        public UIDocument UIDoc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ITransactionFactory TransactionFactory { get; set; }


        /// <summary>
        /// Veritces <see cref="Type"/>'s to stop for graph building.
        /// </summary>
        public IEnumerable<Type> StopTypes { get; set; }

        /// <summary>
        /// Veritces <see cref="Autodesk.Revit.DB.BuiltInCategory"/>'s to stop for graph building.
        /// </summary>
        public Dictionary<BuiltInCategory, List<PartType>> StopCategories { get; set; }

        /// <summary>
        /// Stop on spuds and tees with specified relation.
        /// </summary>
        public IValidator<IVertex> StopRelationValidator { get; set; }

        /// <inheritdoc/>
        public override AdjacencyGraph<IVertex, Edge<IVertex>> Create(Element element)
        {
            _graph = new AdjacencyGraph<IVertex, Edge<IVertex>>();
            _vertexBuilder.Instansiate(_graph);

            var initialVertices = new List<IVertex>();
            var initialEdges = new List<Edge<IVertex>>();

            var gBuilder = _vertexBuilder as GVertexBuilder;
            var firstTaggedVertex = gBuilder?.TryGetVertex(element) is TaggedGVertex<int> firstTagged ?
                firstTagged : default;
            if (firstTaggedVertex.Tag == 0)
            {
                var freeEdge = GetFreeEdge(element as MEPCurve);

                initialVertices.Add(freeEdge.Source);
                initialVertices.Add(freeEdge.Target);
                initialEdges.Add(freeEdge);
            }
            else
            {
                initialVertices.Add(firstTaggedVertex);
            }


            _graph = Create(initialVertices, initialEdges);
            return _graph;
        }

        /// <inheritdoc/>
        public override AdjacencyGraph<IVertex, Edge<IVertex>> Create(MEPCurve mEPCurve, XYZ pointOnMEPCurve)
        {
            _graph = new AdjacencyGraph<IVertex, Edge<IVertex>>();
            _vertexBuilder.Instansiate(_graph);

            var initialVertices = new List<IVertex>();
            var initialEdges = new List<Edge<IVertex>>();

            var v0 = new TaggedGVertex<Point3d>(0, pointOnMEPCurve.ToPoint3d());
            initialVertices.Add(v0);

            var (con1, con2) = mEPCurve.GetMainConnectors();
            var famInstOnCon1 = mEPCurve.GetFirst(pointOnMEPCurve, con1);
            var famInstOnCon2 = mEPCurve.GetFirst(pointOnMEPCurve, con2);

            var gBuilder = _vertexBuilder as GVertexBuilder;

            IVertex v1 = GetVertex(initialVertices.Count, con1, famInstOnCon1, gBuilder);
            initialVertices.Add(v1);
            IVertex v2 = GetVertex(initialVertices.Count, con2, famInstOnCon2, gBuilder);
            initialVertices.Add(v2);


            var e1 = _edgeBuilder.GetEdge(v0, v1, mEPCurve.Id.IntegerValue);
            var e2 = _edgeBuilder.GetEdge(v0, v2, mEPCurve.Id.IntegerValue);

            initialEdges.Add(e1); initialEdges.Add(e2);

            _graph = Create(initialVertices, initialEdges);
            return _graph;

            static IVertex GetVertex(int initialVerticesCount, Connector con, FamilyInstance famInstOnCon, GVertexBuilder gBuilder)
            {
                IVertex vertex;

                if (famInstOnCon == null)
                { vertex = new TaggedGVertex<Point3d>(initialVerticesCount, con.Origin.ToPoint3d()); }
                else
                {
                    vertex = gBuilder?.CreateVertex(initialVerticesCount, famInstOnCon) is TaggedGVertex<int> firstTagged ?
                        firstTagged :
                        default;
                }

                return vertex;
            }
        }

        private Edge<IVertex> GetFreeEdge(MEPCurve mEPCurve)
        {
            var freeCons = ConnectorUtils.GetFreeConnector(mEPCurve);

            var v1 = new TaggedGVertex<Point3d>(0, freeCons[0].Origin.ToPoint3d());
            var v2 = new TaggedGVertex<Point3d>(1, freeCons[1].Origin.ToPoint3d());

            return _edgeBuilder.GetEdge(v1, v2, mEPCurve.Id.IntegerValue);
        }

        private AdjacencyGraph<IVertex, Edge<IVertex>> Create(
            IEnumerable<IVertex> initialVertices,
            IEnumerable<Edge<IVertex>> initialEdges = null)
        {
            var open = new Stack<TaggedGVertex<int>>();

            initialVertices.ForEach(vertex => _graph.AddVertex(vertex));
            (initialEdges ?? new List<Edge<IVertex>>()).ForEach(e => _graph.AddEdge(e));

            var taggedVerices = initialVertices.OfType<TaggedGVertex<int>>().
                Where(v => !v.ContainsTypes(StopTypes, _doc)
                && !v.ContainsCategories(StopCategories, _doc));
            if (StopRelationValidator is not null)
            { taggedVerices = taggedVerices.Where(v => StopRelationValidator.IsValid(v)); }

            taggedVerices.ForEach(open.Push);

            while (open.Count > 0)
            {
                var v1 = open.Pop();
                while (true)
                {
                    var v2 = _vertexBuilder.TryGetVertex(v1);
                    if (v2 == null) { break; }

                    if (v2 is TaggedGVertex<int> tagged)
                    {
                        var foundInOpen = open.FirstOrDefault(v => v.Tag == tagged.Tag);
                        if (foundInOpen.Tag > 0)
                        { v2 = foundInOpen; }
                        else
                        {
                            var t1 = !v2.ContainsTypes(StopTypes, _doc);
                            var t2 = !v2.ContainsCategories(StopCategories, _doc);
                            var t3 = StopRelationValidator is null || StopRelationValidator.IsValid(v2);
                            if (t1 && t2 && t3)
                            { open.Push(tagged); }
                        }
                    };

                    _graph.AddVertex(v2);

                    var edge = _edgeBuilder.GetEdge(v1, v2, _vertexBuilder.EdgeTag);
                    _graph.AddEdge(edge);
                }
            }

            return _graph;
        }
    }
}
