using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
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
using Rhino.UI;
using System;
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

            _visualisator = new AdjacencyGraphVisualisator(_doc)
            {
                ShowElementIds = false,
                ShowVerticesIds = true,
            }
               .Build(Graph);

            Print(Graph);

            //AddAxiliaryPoint(Graph);
            //Show(Graph, _doc, _trfOut);
        }


        public AdjacencyGraph<IVertex, Edge<IVertex>> CreateGraph()
        {
            //var e1 = new ElementSelector(_uiDoc).Pick();

            var selector = new PointSelector(_uiDoc) { AllowLink = true };
            var mEPCurve = selector.Pick() as MEPCurve;
            var point = selector.Point;

            //GVertexBuilder vertexBuilder = GetVertexBuilder();
            GVertexBuilder vertexBuilder = GetVertexBuilderWithValidator();

            var edgeBuilder = new GEdgeBuilder();

            var stopTypes = new List<Type>
            {
                //typeof(MechanicalEquipment)
            };
            var fittingPartTypes = new List<PartType>()
            {
                PartType.Tee,
                   PartType.TapPerpendicular,
                    PartType.TapAdjustable,
                    PartType.SpudPerpendicular,
                    PartType.SpudAdjustable
            };
            var accessoryPartTypes = new List<PartType>() { PartType.Undefined };
            var stopCategories = new Dictionary<BuiltInCategory, List<PartType>>()
            {
                //{ BuiltInCategory.OST_DuctFitting, fittingPartTypes },
                //{ BuiltInCategory.OST_PipeFitting, fittingPartTypes },
                { BuiltInCategory.OST_MechanicalEquipment, accessoryPartTypes }
            };

            var facrory = new MEPSystemGraphFactory(_doc, vertexBuilder, edgeBuilder)
            {
                TransactionFactory = _trfIn,
                UIDoc = _uiDoc,
                StopTypes = stopTypes,
                StopCategories = stopCategories
            };

            //return facrory.Create(e1);
            return facrory.Create(mEPCurve, point);

            GVertexBuilder GetVertexBuilderWithValidator()
            {
                var maxLength = 10000.MMToFeet();
                var maxCount = 100;

                var validator = new VertexLimitsValidator(_doc)
                {
                    MaxLength = maxLength,
                    MaxVerticesCount = maxCount,
                    BoundOutline = null,
                    ExcludedTypes = null,
                    ExculdedCategories = null
                };

                var vertexBuilder = new GVertexBuilder(_doc)
                {
                    Validatator = validator
                };
                return vertexBuilder;
            }

            GVertexBuilder GetVertexBuilder()
            {
                var vertexBuilder = new GVertexBuilder(_doc);
                return vertexBuilder;
            }
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

        public void PairIterate(IVertexListGraph<IVertex, Edge<IVertex>> graph)
        {
            //var algorithm = new DepthFirstSearchAlgorithm<IVertex, Edge<IVertex>>(graph);
            var algorithm = new BreadthFirstSearchAlgorithm<IVertex, Edge<IVertex>>(graph);
            var iterator = new GraphVertexIterator(algorithm);
            var pairIterator = new VertexPairIterator(iterator, (AdjacencyGraph<IVertex, Edge<IVertex>>)graph);


            while (pairIterator.MoveNext())
            {
                using (Transaction transaction = new(_doc, "showPair"))
                {
                    transaction.Start();

                    _visualisator.ShowLocation(pairIterator.Current.Item1);
                    _visualisator.ShowLocation(pairIterator.Current.Item2);
                    _doc.Regenerate();
                    _uiDoc.RefreshActiveView();

                    transaction.RollBack();
                }
                Debug.WriteLine(pairIterator.Current.Item1.Id+ " - " + pairIterator.Current.Item2.Id);
            }

            Debug.WriteLine("Total visited pairs count is: " + pairIterator.Close.Count);
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
            Task task = Task.Run(async () =>
            await trf.CreateAsync(() => _visualisator.Show(),
            "show"));
            _uiDoc.RefreshActiveView();
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
