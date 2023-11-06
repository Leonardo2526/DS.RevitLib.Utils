using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Graphs;
using DS.ClassLib.VarUtils.GridMap;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.Various;
using DS.RevitLib.Utils.Various.Selections;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Search;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class MEPSystemGraphFactoryTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trfIn;
        private readonly ContextTransactionFactory _trfOut;

        public AdjacencyGraph<IVertex, Edge<IVertex>> Graph { get; private set; }

        private IAdjacencyGraphVisulisator<IVertex> _visualisator;

        public MEPSystemGraphFactoryTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = uidoc.Document;

            _trfIn = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
            _trfOut = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Outside);
            Graph = CreateGraph();

            Print(Graph);

            AddAxiliaryPoint(Graph);
            Show(Graph, _doc, _trfOut);
        }


        public AdjacencyGraph<IVertex, Edge<IVertex>> CreateGraph()
        {
            //var e1 = new ElementSelector(_uiDoc).Pick();

            var selector = new PointSelector(_uiDoc) { AllowLink = true };
            var mEPCurve = selector.Pick() as MEPCurve;
            var point = selector.Point;

            var vertexBuilder = new GVertexBuilder(_doc);
            var edgeBuilder = new GEdgeBuilder();
            var facrory = new MEPSystemGraphFactory(_doc, vertexBuilder, edgeBuilder)
            {
                TransactionFactory = _trfIn,
                UIDoc = _uiDoc
            };

            //return facrory.Create(e1);
            return facrory.Create(mEPCurve, point);
        }

        private AdjacencyGraph<IVertex, Edge<IVertex>> AddAxiliaryPoint(AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
            var selector = new PointSelector(_uiDoc) { AllowLink = true };
            var mEPCurve = selector.Pick() as MEPCurve;
            var point = selector.Point;

            graph.TryInsert(mEPCurve, point);

            return graph;
        }

        public void Iterate(IVertexListGraph<IVertex, Edge<IVertex>> graph)
        {
            var algorithm = new BreadthFirstSearchAlgorithm<IVertex, Edge<IVertex>>(graph);
            var iteratror = new GraphVertexIterator(algorithm);
            //var algorithm = new DepthFirstSearchAlgorithm<IVertex, Edge<IVertex>>(bdGraph);

            var txt = "Current vertex id: ";
            while (iteratror.MoveNext())
            {
                _trfIn.CreateAsync(() =>
                { _visualisator.ShowLocation(iteratror.Current); _doc.Regenerate(); }, "show");
                _uiDoc.RefreshActiveView();
                //Task task = Task.Run(async () => 
                //await _trfOut.CreateAsync(() => 
                //{ _visualisator.ShowLocation(iteratror.Current); _uiDoc.RefreshActiveView(); _doc.Regenerate(); },
                //    "show"));                
                Debug.WriteLine(txt + iteratror.Current.ToString());
            }
        }

        private void Print(AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
            foreach (var vertex in graph.Vertices)
            {
                foreach (var edge in graph.OutEdges(vertex))
                {
                    var edgeTag = edge is TaggedEdge<IVertex, int> taggedEdge ? taggedEdge.Tag : 0;
                    var sTag = edge.Source is TaggedGVertex<int> taggedSource ? taggedSource.Tag : 0;
                    var tTag = edge.Target is TaggedGVertex<int> taggedTarget ? taggedTarget.Tag : 0;
                    Debug.WriteLine($"v{edge.Source.Id} elId: {sTag} -> v{edge.Target.Id} elId: {tTag}. Edge: ({edgeTag})");
                }
            }

            Debug.WriteLine("Verices count is: " + graph.Vertices.Count());
            Debug.WriteLine("Edges count is: " + graph.Edges.Count());

            var emptyTagVertices = graph.Vertices.Where(v => v is not TaggedGVertex<int>).ToList();
            var taggedVertices = graph.Vertices.OfType<TaggedGVertex<int>>();
            var distinctVericies = taggedVertices.Distinct(new CompareGTaggedVertex()).ToList();
            var duplicateVertices = taggedVertices.Where(v => !distinctVericies.Contains(v)).ToList();

            Debug.WriteLine("Duplicate vertices count is: " + duplicateVertices.Count);
            duplicateVertices.ForEach(v => Debug.WriteLine(v.Id));

            //Debug.WriteLine("Empty verices count is: " + emptyTagVertices.Count);
            //emptyTagVertices.ForEach(v => Debug.WriteLine(v.Id));


            var taggedEdges = graph.Edges.OfType<TaggedEdge<IVertex, int>>();
            var distinctsEdges = taggedEdges.Distinct(new CompareGTaggedEdge()).ToList();
            var duplicateEdges = taggedEdges.Where(e => !distinctsEdges.Contains(e)).ToList();
            Debug.WriteLine("Duplicate edges count is: " + duplicateEdges.Count);
            duplicateEdges.ForEach(v => Debug.WriteLine(v.Tag));
        }

        private void Show(AdjacencyGraph<IVertex, Edge<IVertex>> graph, Document doc, ITransactionFactory trf)
        {
            _visualisator = new AdjacencyGraphVisualisator(doc)
            {
                ShowElementIds = false,
                ShowVerticesIds = true,
            }
                .Build(graph);

            Task task = Task.Run(async () =>
            await trf.CreateAsync(() => _visualisator.Show(),
            "show"));
        }

        private List<AdjacencyGraph<IVertex, Edge<IVertex>>> Split(AdjacencyGraph<IVertex, Edge<IVertex>> graph, IVertex splitVertex)
        {
            var list = new List<AdjacencyGraph<IVertex, Edge<IVertex>>>();

            var bdGraph = graph.ToBidirectionalGraph();

            bdGraph.TryGetInEdges(splitVertex, out var inEdges);
            bdGraph.TryGetOutEdges(splitVertex, out var outEdges);

            var t = graph.TreeBreadthFirstSearch(splitVertex);


            foreach (var inEdge in inEdges)
            {
                var source1 = inEdge.Source;
                bdGraph.TryGetInEdges(source1, out var in1Edges);
                bdGraph.TryGetOutEdges(source1, out var out1Edges);
            }

            return list;
        }
    }
}
