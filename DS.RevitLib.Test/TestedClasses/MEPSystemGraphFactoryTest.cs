using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Graphs;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.Various;
using QuickGraph;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class MEPSystemGraphFactoryTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trfIn;
        private readonly ContextTransactionFactory _trfOut;

        public MEPSystemGraphFactoryTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = uidoc.Document;

            _trfIn = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
            _trfOut = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Outside);
            AdjacencyGraph<IVertex, Edge<IVertex>> graph = CreateGraph();

            Print(graph);
            Show(graph, _doc, _trfOut);
        }

        public AdjacencyGraph<IVertex, Edge<IVertex>> CreateGraph()
        {
            var e1 = new ElementSelector(_uiDoc).Pick();

            var vertexBuilder = new GVertexBuilder(_doc);
            var edgeBuilder = new GEdgeBuilder();
            var facrory = new MEPSystemGraphFactory(_doc, vertexBuilder, edgeBuilder)
            {
                TransactionFactory = _trfIn,
                UIDoc = _uiDoc
            };

            return facrory.Create(e1);
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
            var visualisator = new AdjacencyGraphVisualisator(doc)
            {
                ShowElementIds = false,
                ShowVerticesIds = true,
            }
                .Build(graph);

            Task task = Task.Run(async () =>
            await trf.CreateAsync(() => visualisator.Show(),
            "show"));
        }
    }
}
