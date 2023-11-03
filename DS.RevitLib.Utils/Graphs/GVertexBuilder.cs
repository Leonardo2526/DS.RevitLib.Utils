using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Represents builder for graph <see cref="GVertex"/> vertices.
    /// </summary>
    public class GVertexBuilder : IVertexBuilder<GVertex, TaggedGVertex<int>>
    {
        private readonly Document _doc;
        private Stack<TaggedGVertex<int>> _open;
        private AdjacencyGraph<GVertex, Edge<GVertex>> _graph;

        /// <summary>
        /// Create builder for graph <see cref="GVertex"/> vertices.
        /// </summary>
        public GVertexBuilder(Document doc)
        {
            _doc = doc;
        }

        /// <inheritdoc/>
        public int EdgeTag { get; private set; }

        /// <inheritdoc/>
        public void Instansiate(Stack<TaggedGVertex<int>> open, AdjacencyGraph<GVertex, Edge<GVertex>> graph)
        {
            _open = open;
            _graph = graph;
        }

        /// <summary>
        /// Try to get a new <see cref="GVertex"/> from <paramref name="parentVertex"/>.
        /// </summary>
        /// <param name="parentVertex"></param>
        /// <returns>
        /// A new <see cref="GVertex"/> that graph doesn't contains yet.
        /// </returns>
        public GVertex TryGetVertex(TaggedGVertex<int> parentVertex)
        {
            GVertex v2 = null;
            EdgeTag = 0;

            var pvFamInst = GetFamInst(parentVertex);
            var pvConnected = pvFamInst.GetBestConnected();
            var pVExcluded = GetExcluded(parentVertex, pvFamInst, _graph);
            var elementsToAdd = pvConnected.Where(e => !pVExcluded.Contains(e.Id));

            foreach (var e in elementsToAdd)
            {
                v2 = TryGetVertex(e, parentVertex);
                if (v2 != null) break;
            }

            return v2;

            GVertex TryGetVertex(Element elementToAdd, TaggedGVertex<int> parentVertex)
            {
                GVertex v2 = null;

                switch (elementToAdd)
                {
                    case MEPCurve mEPCurve:
                        {
                            EdgeTag = mEPCurve.Id.IntegerValue;
                            var (con1, con2) = mEPCurve.GetMainConnectors();
                            var pvLocation = pvFamInst.GetLocation();

                            var ex = new List<ElementId>() { new ElementId(parentVertex.Tag) };

                            var famInstOnCon1 = mEPCurve.GetFirst(pvLocation, con1, ex);
                            FamilyInstance famInstToAdd1 = null;
                            if (famInstOnCon1 != null)
                            {
                                famInstToAdd1 = pVExcluded.Contains(famInstOnCon1.Id) ? null : famInstOnCon1;
                                if (famInstToAdd1 != null)
                                { v2 = CreateVertex(_graph.VertexCount, famInstToAdd1); break; }
                            }

                            var famInstOnCon2 = mEPCurve.GetFirst(pvLocation, con2, ex);
                            FamilyInstance famInstToAdd2 = null;
                            if (famInstOnCon2 != null)
                            {
                                famInstToAdd2 = pVExcluded.Contains(famInstOnCon2.Id) ? null : famInstOnCon2;
                                if (famInstToAdd2 != null)
                                { v2 = CreateVertex(_graph.VertexCount, famInstToAdd2); break; }
                            }

                            if (famInstToAdd1 == null && famInstToAdd2 == null)
                            {
                                var location = TryGetFreePoint(mEPCurve, _graph, EdgeTag, parentVertex);
                                if (!double.IsNaN(location.X))
                                { v2 = new TaggedGVertex<Point3d>(_graph.VertexCount, location); }
                            }
                            else
                            { v2 = CreateVertex(_graph.VertexCount, famInstToAdd1 ?? famInstToAdd2); }
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
        }

        /// <inheritdoc/>
        public GVertex TryGetVertex(Element element)
        {
            FamilyInstance familyInstance = element as FamilyInstance ??
                element.GetBestConnected().FirstOrDefault() as FamilyInstance;

            return familyInstance == null ?
                new GVertex(_graph.VertexCount) :
                CreateVertex(_graph.VertexCount, familyInstance);
        }


        #region PrivateMethods

        /// <summary>
        /// Try to get <see cref="GVertex"/> by <paramref name="element"/>
        /// </summary>
        /// <param name="element"></param>
        /// <returns>
        /// <see cref="TaggedGVertex{TTag}"/>built by <paramref name="element"/> it's <see cref="Autodesk.Revit.DB.FamilyInstance"/>.        
        /// <para>
        /// <see cref="TaggedGVertex{TTag}"/> built by first connected <see cref="Autodesk.Revit.DB.FamilyInstance"/> 
        /// if <paramref name="element"/> is <see cref="MEPCurve"/>.
        /// </para>
        /// <para>
        /// <see cref="GVertex"/> if <paramref name="element"/> is <see cref="MEPCurve"/>, hasn't connected and elements.
        /// </para>
        /// <para>
        /// Otherwise <see langword="null"/>
        /// </para>
        /// </returns>       
        private List<ElementId> GetExcluded(TaggedGVertex<int> pv, FamilyInstance pvFamInst,
            AdjacencyGraph<GVertex, Edge<GVertex>> graph)
        {
            var excluded = new List<ElementId>() { pvFamInst.Id };

            if (pvFamInst.IsSpud())
            {
                var spudExcluded = GetSpudExcluded(pv, graph, pvFamInst);
                excluded.AddRange(spudExcluded);
            }
            else
            {
                var existPvEdges = GetLinkedEdges(pv, graph);
                foreach (var existEdge in existPvEdges)
                {
                    TryAddEdgeVerticesToExcluded(pv, excluded, existEdge);
                    if (existEdge is TaggedEdge<GVertex, int> taggedEdge)
                    { excluded.Add(new ElementId(taggedEdge.Tag)); }
                }
            }

            return excluded;

            static void TryAddEdgeVerticesToExcluded(TaggedGVertex<int> pv, List<ElementId> excluded, Edge<GVertex> pvEdge)
            {
                if (pvEdge.Source is TaggedGVertex<int> sourseTagged && sourseTagged.Tag != pv.Tag)
                { excluded.Add(new ElementId(sourseTagged.Tag)); }

                if (pvEdge.Target is TaggedGVertex<int> targetTagged && targetTagged.Tag != pv.Tag)
                { excluded.Add(new ElementId(targetTagged.Tag)); }
            }

            List<ElementId> GetSpudExcluded(TaggedGVertex<int> pv, AdjacencyGraph<GVertex, Edge<GVertex>> graph, FamilyInstance spud)
            {
                var excluded = new List<ElementId>();

                //find parent MEPCurve
                (List<Element> parents, Element child) = spud.GetConnectedElements(true);

                //add exist children edges
                var existChildrenEdge = GetLinkedEdges(pv, graph).
                    OfType<TaggedEdge<GVertex, int>>().
                    FirstOrDefault(e => e.Tag == child.Id.IntegerValue);
                if (existChildrenEdge != null)
                { excluded.Add(new ElementId(existChildrenEdge.Tag)); }

                var parentMEPCurve = parents.OfType<MEPCurve>().FirstOrDefault();
                if (parentMEPCurve == null) { return excluded; }

                //add all exist vertices in graph with edge tag as parentMEPCurve's tag
                var existEdgesOnParentMEPCurve = graph.Edges.
                    OfType<TaggedEdge<GVertex, int>>().
                    Where(e => e.Tag == parentMEPCurve.Id.IntegerValue).ToList();
                existEdgesOnParentMEPCurve.ForEach(e => TryAddEdgeVerticesToExcluded(pv, excluded, e));

                return excluded;
            }
        }

        private TaggedGVertex<int> CreateVertex(int vertexId, FamilyInstance familyInstance)
        => new(vertexId, familyInstance.Id.IntegerValue);

        private FamilyInstance GetFamInst(TaggedGVertex<int> vertex) =>
            _doc.GetElement(new ElementId(vertex.Tag)) as FamilyInstance;

        private List<Edge<GVertex>> GetLinkedEdges(TaggedGVertex<int> vertex, AdjacencyGraph<GVertex, Edge<GVertex>> graph)
        {
            var edges = new List<Edge<GVertex>>();

            graph.TryGetOutEdges(vertex, out var outEdges);
            var inEdges = GetInEdges(vertex);

            edges.AddRange(inEdges);
            edges.AddRange(outEdges);

            return edges;
        }

        private IEnumerable<Edge<GVertex>> GetInEdges(GVertex vertex) =>
            _graph.Edges.Where(e => e.Target.Id == vertex.Id);

        private IEnumerable<GVertex> UntaggedVertices(AdjacencyGraph<GVertex, Edge<GVertex>> graph, int edgeTag, TaggedGVertex<int> sourceVertex)
        {
            var existVertices = new List<GVertex>();

            var existEdgesWithUntaggedTarget = graph.Edges.
                OfType<TaggedEdge<GVertex, int>>().
                Where(e => e.Tag == edgeTag
                && e.Target is not TaggedGVertex<int>
                && e.Source.Id == sourceVertex.Id);
            existEdgesWithUntaggedTarget.ForEach(e => existVertices.Add(e.Target));

            return existVertices;
        }


        private Point3d TryGetFreePoint(MEPCurve mEPCurve,
            AdjacencyGraph<GVertex, Edge<GVertex>> graph, int edgeTag,
            TaggedGVertex<int> parentVertex)
        {
            var freeCons = ConnectorUtils.GetFreeConnector(mEPCurve);
            if (freeCons.Count > 0)
            {
                var excludedVertexPoins = GetExcludedPoints(graph, edgeTag, parentVertex);
                if(excludedVertexPoins.Count() ==0) { return freeCons[0].Origin.ToPoint3d(); }

                foreach (var c in freeCons)
                {
                    var conPoint = c.Origin.ToPoint3d();
                    if(excludedVertexPoins.Any(p => p.Tag.DistanceTo(conPoint) < 0.001))
                    { continue; }
                    else
                    {  return conPoint; }
                }
            }

            return  new Point3d(double.NaN, double.NaN, double.NaN);

            IEnumerable<TaggedGVertex<Point3d>> GetExcludedPoints(
                AdjacencyGraph<GVertex, Edge<GVertex>> graph,
                int edgeTag, TaggedGVertex<int> sourceVertex)
            {
                var excluded = new List<TaggedGVertex<Point3d>>();

                var existEdgesWithUntaggedTarget = graph.Edges.
                    OfType<TaggedEdge<GVertex, int>>().
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
