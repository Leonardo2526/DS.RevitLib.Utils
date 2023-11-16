using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using DS.RevitLib.Utils.Various;
using DS.RevitLib.Utils.Various.Selections;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class MEPSystemGraphFactoryTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trfIn;
        private readonly ContextTransactionFactory _trfOut;

        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> Graph { get; private set; }

        private IAdjacencyGraphVisulisator<IVertex> _visualisator;

        public MEPSystemGraphFactoryTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = uidoc.Document;

            _trfIn = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
            _trfOut = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Outside);
            //Graph = CreateGraphByElelment();
            Graph = CreateGraphByPoint();

            _visualisator = new AdjacencyGraphVisualisator(_doc)
            {
                ShowElementIds = false,
                ShowVerticesIds = true,
                ShowDirecionts = true
            }
               .Build(Graph);

            //Print(Graph);

            //var trimmedGraph = TrimTest(Graph);
            //Print(trimmedGraph);

            //_visualisator.Build(trimmedGraph);
            //AddAxiliaryPoint(Graph);
            //Show(Graph, _doc, _trfOut);
        }

        public AdjacencyGraph<IVertex, Edge<IVertex>> CreateGraphByElelment()
        {
            var e1 = new ElementSelector(_uiDoc).Pick();

            GVertexBuilder vertexBuilder = GetVertexBuilder();

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

            return facrory.Create(e1);

            GVertexBuilder GetVertexBuilder()
            {
                var vertexBuilder = new GVertexBuilder(_doc);
                return vertexBuilder;
            }
        }

        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> CreateGraphByPoint()
        {
            var selector = new PointSelector(_uiDoc) { AllowLink = true };
            var mEPCurve = selector.Pick() as MEPCurve;
            var point = selector.Point;
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

            var maxLength = 100000.MMToFeet();
            var maxCount = 100;

            var builder = new MEPGraphBuilder(_doc)
            {
                StopTypes = stopTypes,
                StopCategories = stopCategories,
                StopElementRelation = Relation.Parent,
                MaxLength = maxLength,
                MaxVerticesCount = maxCount,
                BoundOutline = null,
                ExcludedTypes = null,
                //ExculdedCategories = exculdedCategories,
            };

            return builder.Create(mEPCurve, point);
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
                _trfIn.CreateAsync((Action)(() =>
                { _visualisator.Show(iteratror.Current); _doc.Regenerate(); }), "show");
                _uiDoc.RefreshActiveView();
                //Task task = Task.Run(async () => 
                //await _trfOut.CreateAsync(() => 
                //{ _visualisator.ShowLocation(iteratror.Current); _uiDoc.RefreshActiveView(); _doc.Regenerate(); },
                //    "show"));                
                Debug.WriteLine(txt + iteratror.Current.ToString());
            }
        }

        public void PairIterate(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            //var trimmedGraph = graph;
            //var trimmedGraph = graph.Trim(_doc, stopCategories);

            var cats = GetIterationCategories();

            var algorithm = new BreadthFirstSearchAlgorithm<IVertex, Edge<IVertex>>(graph);

            var startRoot = graph.Vertices.ToList()[1];
            var catValidator = new VertexFamInstCategoryValidator(_doc, cats);

            var bdGraph = graph.ToBidirectionalGraph();
            var relationValidator = new VertexRelationValidator(_doc, bdGraph)
            {
                InElementRelation = Relation.Child
            };

            var collisionValidator = GetCollisionValidator(_doc, bdGraph);

            var iterator = new GraphVertexIterator(algorithm)
            {
                StartIndex = 1
            };
            iterator.Validators.Add(catValidator);
            iterator.Validators.Add(relationValidator);
            iterator.Validators.Add(collisionValidator);

            var pairIterator = new VertexPairIterator(iterator, graph);


            while (pairIterator.MoveNext())
            {
                using (Transaction transaction = new(_doc, "showPair"))
                {
                    transaction.Start();

                    _visualisator.Show(pairIterator.Current.Item1);
                    _visualisator.Show(pairIterator.Current.Item2);
                    _doc.Regenerate();
                    _uiDoc.RefreshActiveView();

                    transaction.RollBack();
                }
                Debug.WriteLine(pairIterator.Current.Item1.Id + " - " + pairIterator.Current.Item2.Id);
            }

            Debug.WriteLine("Total visited pairs count is: " + pairIterator.Close.Count);
        }

        private AdjacencyGraph<IVertex, Edge<IVertex>> TrimTest(AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
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
                { BuiltInCategory.OST_DuctFitting, fittingPartTypes },
                { BuiltInCategory.OST_PipeFitting, fittingPartTypes },
                //{ BuiltInCategory.OST_MechanicalEquipment, accessoryPartTypes }
            };

            return graph.Trim(_doc, stopCategories);
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

        private async void Show(IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph, Document doc, ITransactionFactory trf)
        {
            Task task = Task.Run(async () =>
            await trf.CreateAsync(() => _visualisator.Show(),
            "show"));
            _uiDoc.RefreshActiveView();
            await task;
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

        private Dictionary<BuiltInCategory, List<PartType>> GetIterationCategories()
        {
            var fittingPartTypes = new List<PartType>()
            {
                PartType.Elbow,
                PartType.Tee,
                PartType.TapPerpendicular,
                PartType.TapAdjustable,
                PartType.SpudPerpendicular,
                PartType.SpudAdjustable
            };
            var verificationCategories = new Dictionary<BuiltInCategory, List<PartType>>()
            {
                { BuiltInCategory.OST_DuctFitting, fittingPartTypes },
                { BuiltInCategory.OST_PipeFitting, fittingPartTypes },
            };

            return verificationCategories;
        }

        private IValidator<IVertex> GetCollisionValidator(Document doc, IBidirectionalGraph<IVertex, Edge<IVertex>> bdGraph)
        {
            var factory = new ElementIntersectionFactory(doc);
            var elementCollisionDetector = new ElementCollisionDetector(doc, factory);
            var xYZCollisionDetector = new XYZCollisionDetector(elementCollisionDetector);

            return new VertexCollisionValidator(_doc, bdGraph, elementCollisionDetector, xYZCollisionDetector);
        }


        public void SortTest(AdjacencyGraph<IVertex, Edge<IVertex>> graph)
        {
            AddAxiliaryPoint(graph);
            AddAxiliaryPoint(graph);

            //Print(graph);
            //Show(graph, _doc, _trfOut);
            //return;

            //var algorithm = new BreadthFirstSearchAlgorithm<IVertex, Edge<IVertex>>(graph);

            var cats = GetIterationCategories();
            //var catValidator = new VertexFamInstCategoryValidator(_doc, cats);
            //var bdGraph = graph.ToBidirectionalGraph();
            //var relationValidator = new VertexRelationValidator(_doc, bdGraph)
            //{
            //    InElementRelation = Relation.Child
            //};
            //var iterator = new GraphVertexIterator(algorithm)
            //{
            //    StartIndex = 1
            //};
            //iterator.Validators.Add(catValidator);
            //iterator.Validators.Add(relationValidator);

            //var pairIterator = new VertexPairIterator(iterator, graph);


            var pairIterator = new PairIteratorBuilder(_doc)
            {
                StartIndex = 1,
                AvailableCategories = cats,
                InElementRelation = Relation.Child
            }
            .Create(graph);

            List<(IVertex, IVertex)> list = new();

            while (list.Count != 4 && pairIterator.MoveNext())
            { list.Add(pairIterator.Current); }

            var firstEdge = graph.Edges.First() as TaggedEdge<IVertex, int>;
            //return;
            var baseElement = _doc.GetElement(new ElementId(firstEdge.Tag)) as MEPCurve;
            double sizeFactor = baseElement.GetMaxSize();
            var sortedList = list.SortByTaggedLength(graph, _doc, 25, sizeFactor).ToList();

            foreach (var pair in sortedList)
            {
                using (Transaction transaction = new(_doc, "showPair"))
                {
                    transaction.Start();

                    _visualisator.Show(pair.Item1);
                    _visualisator.Show(pair.Item2);
                    _doc.Regenerate();
                    _uiDoc.RefreshActiveView();

                    transaction.RollBack();
                }
                Debug.WriteLine(pair.Item1.Id + " - " + pair.Item2.Id);
            }


            while (pairIterator.MoveNext())
            {
                using (Transaction transaction = new(_doc, "showPair"))
                {
                    transaction.Start();

                    _visualisator.Show(pairIterator.Current.Item1);
                    _visualisator.Show(pairIterator.Current.Item2);
                    _doc.Regenerate();
                    _uiDoc.RefreshActiveView();

                    transaction.RollBack();
                }
                Debug.WriteLine(pairIterator.Current.Item1.Id + " - " + pairIterator.Current.Item2.Id);
            }

            var ir = pairIterator as VertexPairIterator;
            Debug.WriteLine("Total visited pairs count is: " + ir.Close.Count);
        }
    }
}
