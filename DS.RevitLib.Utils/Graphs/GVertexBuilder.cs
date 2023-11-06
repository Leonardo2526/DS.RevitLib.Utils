using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Graphs;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP;
using MoreLinq;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Represents builder for graph <see cref="IVertex"/> vertices.
    /// </summary>
    public class GVertexBuilder : IVertexBuilder<IVertex, TaggedGVertex<int>>
    {
        private readonly Document _doc;
        private AdjacencyGraph<IVertex, Edge<IVertex>> _graph;

        /// <summary>
        /// Create builder for graph <see cref="IVertex"/> vertices.
        /// </summary>
        public GVertexBuilder(Document doc)
        {
            _doc = doc;
        }

        /// <inheritdoc/>
        public int EdgeTag { get; private set; }

        /// <inheritdoc/>
        public void Instansiate(AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// Try to get a new <see cref="IVertex"/> from <paramref name="parentVertex"/>.
        /// </summary>
        /// <param name="parentVertex"></param>
        /// <returns>
        /// A new <see cref="IVertex"/> that graph doesn't contains yet.
        /// </returns>
        public IVertex TryGetVertex(TaggedGVertex<int> parentVertex)
        {
            IVertex v2 = null;
            EdgeTag = 0;

            var pvFamInst = _doc.GetElement(new ElementId(parentVertex.Tag)) as FamilyInstance;
            var pvConnected = pvFamInst.GetBestConnected();
            var pVExcluded = GetExcluded(parentVertex, pvFamInst, _graph);
            var elementsToAdd = pvConnected.Where(e => !pVExcluded.Contains(e.Id));

            foreach (var e in elementsToAdd)
            {
                v2 = TryGetVertex(e, parentVertex, pvFamInst, pVExcluded);
                if (v2 != null) break;
            }

            return v2;

        }

        /// <inheritdoc/>
        public IVertex TryGetVertex(Element element)
        {
            FamilyInstance familyInstance = element as FamilyInstance ??
                element.GetBestConnected().FirstOrDefault() as FamilyInstance;

            return familyInstance == null ?
                null :
                CreateVertex(_graph.VertexCount, familyInstance);
        }

        public TaggedGVertex<int> CreateVertex(int vertexId, FamilyInstance familyInstance)
        => new(vertexId, familyInstance.Id.IntegerValue);


        #region PrivateMethods

        private IVertex TryGetVertex(Element elementToAdd, TaggedGVertex<int> parentVertex,
            FamilyInstance pvFamInst, IEnumerable<ElementId> pVExcluded)
        {
            IVertex v2 = null;

            switch (elementToAdd)
            {
                case MEPCurve mEPCurve:
                    {
                        EdgeTag = mEPCurve.Id.IntegerValue;
                        var (con1, con2) = mEPCurve.GetMainConnectors();
                        var pvLocation = pvFamInst.GetLocation();

                        FamilyInstance famInstToAdd =
                            TryGetFamilyInstToAdd(mEPCurve, pvLocation, con1, pVExcluded, parentVertex) ??
                            TryGetFamilyInstToAdd(mEPCurve, pvLocation, con2, pVExcluded, parentVertex);
                        if (famInstToAdd == null)
                        {
                            var location = TryGetFreePoint(mEPCurve, _graph, EdgeTag, parentVertex);
                            if (!double.IsNaN(location.X))
                            { v2 = new TaggedGVertex<Point3d>(_graph.VertexCount, location); }
                        }
                        else
                        { v2 = CreateVertex(_graph.VertexCount, famInstToAdd); }
                        break;
                    }
                case FamilyInstance familyInstance:
                    {
                        v2 = CreateVertex(_graph.VertexCount, familyInstance);
                        break;
                    }
                default: throw new NotImplementedException();
            }

            return v2;

        }


        private FamilyInstance TryGetFamilyInstToAdd(MEPCurve mEPCurve,
             XYZ pvLocation, Connector con,
             IEnumerable<ElementId> pVExcluded, TaggedGVertex<int> parentVertex)
        {
            var ex = new List<ElementId>() { new ElementId(parentVertex.Tag) };
            var famInstOnCon = mEPCurve.GetFirst(pvLocation, con, ex);
            if (famInstOnCon == null) { return null; }
            return pVExcluded.Contains(famInstOnCon.Id) ?
                    null :
                    famInstOnCon;
        }

        private List<ElementId> GetExcluded(TaggedGVertex<int> pv, FamilyInstance pvFamInst,
            AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
            var excluded = new List<ElementId>() { pvFamInst.Id };

            if (pvFamInst.IsSpud())
            {
                var spudExcluded = GetSpudExcluded(pv, graph, pvFamInst);
                excluded.AddRange(spudExcluded);
            }
            else
            {
                var existPvEdges = GetIncidentEdges(pv, graph);
                foreach (var existEdge in existPvEdges)
                {
                    TryAddEdgeVerticesToExcluded(pv, excluded, existEdge);
                    if (existEdge is TaggedEdge<IVertex, int> taggedEdge)
                    { excluded.Add(new ElementId(taggedEdge.Tag)); }
                }
            }

            return excluded;

            static void TryAddEdgeVerticesToExcluded(TaggedGVertex<int> pv, List<ElementId> excluded, Edge<IVertex> pvEdge)
            {
                if (pvEdge.Source is TaggedGVertex<int> sourseTagged && sourseTagged.Tag != pv.Tag)
                { excluded.Add(new ElementId(sourseTagged.Tag)); }

                if (pvEdge.Target is TaggedGVertex<int> targetTagged && targetTagged.Tag != pv.Tag)
                { excluded.Add(new ElementId(targetTagged.Tag)); }
            }

            List<ElementId> GetSpudExcluded(TaggedGVertex<int> pv, AdjacencyGraph<IVertex, Edge<IVertex>> graph, FamilyInstance spud)
            {
                var excluded = new List<ElementId>();

                //find parent MEPCurve
                (List<Element> parents, Element child) = spud.GetConnectedElements(true);

                //add exist children edges
                var existChildrenEdge = GetIncidentEdges(pv, graph).
                    OfType<TaggedEdge<IVertex, int>>().
                    FirstOrDefault(e => e.Tag == child.Id.IntegerValue);
                if (existChildrenEdge != null)
                { excluded.Add(new ElementId(existChildrenEdge.Tag)); }

                var parentMEPCurve = parents.OfType<MEPCurve>().FirstOrDefault();
                if (parentMEPCurve == null) { return excluded; }

                //add all exist vertices in graph with edge tag as parentMEPCurve's tag
                var existEdgesOnParentMEPCurve = graph.Edges.
                    OfType<TaggedEdge<IVertex, int>>().
                    Where(e => e.Tag == parentMEPCurve.Id.IntegerValue).ToList();
                existEdgesOnParentMEPCurve.ForEach(e => TryAddEdgeVerticesToExcluded(pv, excluded, e));

                return excluded;
            }
        }


        private List<Edge<IVertex>> GetIncidentEdges(TaggedGVertex<int> vertex, AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
            var edges = new List<Edge<IVertex>>();

            graph.TryGetOutEdges(vertex, out var outEdges);
            var inEdges = _graph.Edges.Where(e => e.Target.Id == vertex.Id);

            edges.AddRange(inEdges);
            edges.AddRange(outEdges);

            return edges;
        }

        private Point3d TryGetFreePoint(MEPCurve mEPCurve,
            AdjacencyGraph<IVertex, Edge<IVertex>> graph, int edgeTag,
            TaggedGVertex<int> parentVertex)
        {
            var freeCons = ConnectorUtils.GetFreeConnector(mEPCurve);
            if (freeCons.Count > 0)
            {
                var excludedVertexPoins = GetExcludedPoints(graph, edgeTag, parentVertex);
                if (excludedVertexPoins.Count() == 0) { return freeCons[0].Origin.ToPoint3d(); }

                foreach (var c in freeCons)
                {
                    var conPoint = c.Origin.ToPoint3d();
                    if (excludedVertexPoins.Any(p => p.Tag.DistanceTo(conPoint) < 0.001))
                    { continue; }
                    else
                    { return conPoint; }
                }
            }

            return new Point3d(double.NaN, double.NaN, double.NaN);

            IEnumerable<TaggedGVertex<Point3d>> GetExcludedPoints(
                AdjacencyGraph<IVertex, Edge<IVertex>> graph,
                int edgeTag, TaggedGVertex<int> sourceVertex)
            {
                var excluded = new List<TaggedGVertex<Point3d>>();

                var existEdgesWithUntaggedTarget = graph.Edges.
                    OfType<TaggedEdge<IVertex, int>>().
                    Where(e => e.Tag == edgeTag);
                foreach (var e in existEdgesWithUntaggedTarget)
                {
                    if (e.Target is TaggedGVertex<Point3d> tTagget)
                    { excluded.Add(tTagget); }
                    if (e.Source is TaggedGVertex<Point3d> tSource)
                    { excluded.Add(tSource); }
                }

                return excluded;
            }
        }


        #endregion

    }
}
