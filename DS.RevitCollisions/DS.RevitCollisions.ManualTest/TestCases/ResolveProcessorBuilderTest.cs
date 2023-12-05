using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.CollisionBuilers;
using DS.RevitCollisions.Models;
using DS.RevitCollisions.Resolve.ResolveFactories;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using DS.RevitLib.Utils.PathCreators.AlgorithmVertexBuilder;
using DS.RevitLib.Utils.Various;
using QuickGraph;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DS.RevitCollisions.ManualTest.TestCases
{

    internal class ResolveProcessorBuilderTest
    {
        private readonly bool _autoTask = false;
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trfIn;
        private readonly ContextTransactionFactory _trfOut;
        private readonly ContextTransactionFactory _trfAuto;
        private readonly UIApplication _uiApp;
        private ILogger _logger;
        private ElementCollisionFactory _factory;
        private readonly ICollisionVisualizator<Collision> _collisionVisualizator;
        private readonly VertexPairVisualizator _taskVisualizator;
        private readonly IItemVisualisator<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> _graphVisualisator;
        private readonly ITraceSettings _traceSettings;
        private readonly ElementCollisionDetector _collisionDetector;

        public ResolveProcessorBuilderTest(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _uiDoc = uiApp.ActiveUIDocument;
            _doc = _uiDoc.Document;
            _trfIn = new ContextTransactionFactory(_doc, RevitContextOption.Inside);
            _trfOut = new ContextTransactionFactory(_doc, RevitContextOption.Outside);
            _trfAuto = new ContextTransactionFactory(_doc);


            _logger = new LoggerConfiguration()
                 .MinimumLevel.Information()
                 .WriteTo.Debug()
                 .CreateLogger();

            _traceSettings = new TraceSettings();

            _collisionDetector = new ElementCollisionDetector(_doc, new ElementIntersectionFactory(_doc));
            _collisionVisualizator = new CollisionVisualizator(uiApp);

            _taskVisualizator = new VertexPairVisualizator(_uiDoc)
            {
                TransactionFactory = _trfAuto,
                RefreshView = true,
            };

            _graphVisualisator = new GraphVisulisator(_uiDoc)
            {
                ShowElementIds = false,
                ShowVerticesIds = false,
                ShowDirecionts = true,
                TransactionFactory = _trfAuto,
                RefreshView = true
            };
        }

        public Collision Collision { get; private set; }

        public void RunTest1()
        {
            var processor = CreateProcessor(true);
            var mEPCollision = Collision as IMEPCollision;
            var result = processor?.TryResolve();
        }

        public async Task RunTest1ASync()
        {
            var processor = CreateProcessor(true);
            var mEPCollision = Collision as IMEPCollision;
            var result = await processor.TryResolveAsync();
            //_graphVisualisator?.Show(result);

            //await _graphVisualisator?.ShowAsync(result);
            //await Task.Run(async () => await RevitTask.RunAsync(() => _graphVisualisator?.Show(result)));
            //await RevitTask.RunAsync(() => TestTransaction(_doc));
        }

        public void RunTestMany()
        {
            var processor = CreateProcessor(true);
            var mEPCollision = Collision as IMEPCollision;

            var result = processor.TryResolve(); ;
            while (result != null)
            {
                result = processor.TryResolve();
                _uiDoc.RefreshActiveView();
            }
        }

        private ResolveProcessor<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> CreateProcessor(bool insertAxiliaryPoints = false)
        {
            _factory = BuildCollisionFactory();
            Collision = CreateCollision(_factory);
            //_collisionVisualizator.Show(Collision);


            var mEPCollision = Collision as MEPCollision;

            //build Graph
            var graph = GetGraph(_doc, mEPCollision);

            //_graphVisualisator.Show(graph);
            //graph.PrintEdges();
            return GetProcessor();

            return null;

            ResolveProcessor<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> GetProcessor()
            {
                //Create and config pathFind factory
                var pathFindFactory = new XYZPathFinderFactory(_uiDoc)
                {
                    TraceSettings = _traceSettings,
                };
                var pathFinder = pathFindFactory.GetPathFinder();
                pathFinder.AccountInitialDirections = true;
                pathFinder.MinimizePathNodes = true;
                pathFinder.AllowSecondElementForBasis = true;
                pathFinder.MaxTime = 1000000;
                pathFinder.ExternalOutline = null;
                pathFinder.InsulationAccount = true;
                pathFinder.AllowSecondElementForBasis = false;
                pathFinder.OutlineFactory = null;

                IResolveFactory<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> resolveFactory;
                if (_autoTask)
                {
                    resolveFactory = new PathFindCollisionFactoryBuilder(_uiDoc, _collisionDetector, graph, pathFinder, mEPCollision)
                    {
                        TraceSettings = _traceSettings,
                        IterationCategories = GetIterationCategories(),
                        Logger = _logger,
                        TaskVisualizator = _taskVisualizator,
                        ResultVisualizator = _graphVisualisator,
                        ResolveParallel = true,
                        Messenger = new TaskDialogMessenger(),
                        TransactionFactory = _trfAuto
                    }.Create();
                }
                else
                {
                    resolveFactory = new PathFindGraphFactoryBuilder(_uiDoc, _collisionDetector, graph, pathFinder,
                        mEPCollision.Item1Model.MEPCurve, mEPCollision.Item1Model.MEPCurve, mEPCollision.Item2 as MEPCurve)
                    {
                        TraceSettings = _traceSettings,
                        IterationCategories = GetIterationCategories(),
                        Logger = _logger,
                        TaskVisualizator = _taskVisualizator,
                        ResultVisualizator = _graphVisualisator,
                        ResolveParallel = true,
                        Messenger = new TaskDialogMessenger(),
                        TransactionFactory = _trfAuto
                    }.Create();
                }

                //add factories
                var factories = new List<IResolveFactory<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>>>
            {
                resolveFactory
            };

                return
                    new ResolveProcessor<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>>(factories)
                    {
                        Logger = _logger
                    };
            }
        }

        public Collision CreateCollision(ElementCollisionFactory factory)
        {
            var e1 = new ElementSelector(_uiDoc).Pick();
            var e2 = new ElementSelector(_uiDoc).Pick();

            return factory.CreateCollision(e1, e2);
        }

        private ElementCollisionFactory BuildCollisionFactory()
        {
            var factory = new ElementCollisionFactory()
            {
                Visualizator = _collisionVisualizator,
                ExcludeTraversableArchitecture = true,
                MinIntersectionVolume = 0,
                Logger = _logger
            };

            return factory;
        }

        private AdjacencyGraph<IVertex, Edge<IVertex>> GetGraph(Document doc, MEPCollision mEPCollision)
        {
            var stopCategories = new Dictionary<BuiltInCategory, List<PartType>>()
            {
                { BuiltInCategory.OST_MechanicalEquipment,
                    new List<PartType>() { PartType.Undefined } }
            };

            var builder = new MEPGraphBuilder(doc)
            {
                StopCategories = stopCategories,
                StopElementRelation = Relation.Parent,
                MaxLength = 20000.MMToFeet(),
                MaxVerticesCount = 100,
                BoundOutline = null,
                ExcludedTypes = null,
                //ExculdedCategories = exculdedCategories,
            };

            var mEPCurve = mEPCollision.Item1Model.MEPCurve;
            var point = mEPCollision.IntersectionSolid.Center;
            var graph = builder.Create(mEPCurve, point) as AdjacencyGraph<IVertex, Edge<IVertex>>;
            { Log.Information($"Graph with {graph.VertexCount} vertices and {graph.EdgeCount} edges was built successfully."); }

            return graph;
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


        private void TestTransaction(Document doc, bool commitTransaction = true)
        {

            using (Transaction transaction = new Transaction(doc, "test"))
            {
                transaction.Start();

                doc.Regenerate();

                if (transaction.HasStarted())
                {
                    if (commitTransaction)
                    { transaction.Commit(); }
                    else
                    { transaction.RollBack(); }
                }
            }
        }
    }
}
