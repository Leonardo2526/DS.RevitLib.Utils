﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Resolvers;
using DS.RevitCollisions.CollisionBuilers;
using DS.RevitCollisions.Impl;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Various;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Graphs;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using QuickGraph;
using DS.RevitLib.Utils.Visualisators;
using DS.ClassLib.VarUtils.GridMap;
using System.Security.Cryptography;
using QuickGraph.Algorithms;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using static System.Net.Mime.MediaTypeNames;
using DS.RevitLib.Utils.Elements.MEPElements;
using Rhino.UI;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.PathCreators;
using DS.RevitLib.Utils.PathCreators.AlgorithmBuilder;
using DS.RevitLib.Utils.Bases;
using DS.PathFinder;
using Serilog.Core;
using DS.RevitLib.Utils.PathCreators.AlgorithmVertexBuilder;

namespace DS.RevitCollisions.ManualTest.TestCases
{

    internal class ResolveProcessorBuilderTest
    {
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

            _taskVisualizator = new VertexPairVisualizator(_doc)
            {
                TransactionFactory = _trfAuto,
                UiDoc = _uiDoc
            };

            _graphVisualisator = new GraphVisulisator(_doc)
            {
                ShowElementIds = false,
                ShowVerticesIds = false,
                ShowDirecionts = true,
                TransactionFactory = _trfAuto
            };
        }

        public Collision Collision { get; private set; }

        public void RunTest1()
        {
            var processor = CreateProcessor(true);
            var mEPCollision = Collision as IMEPCollision;
            var result = processor.TryResolve(mEPCollision);
        }

        public void RunTestMany()
        {
            var processor = CreateProcessor(true);
            var mEPCollision = Collision as IMEPCollision;

            var result = processor.TryResolve(mEPCollision); ;
            while (result != null)
            {
                result = processor.TryResolve(mEPCollision);
                _uiDoc.RefreshActiveView();
            }
        }

        private ResolveProcessor<IMEPCollision, IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> CreateProcessor(bool insertAxiliaryPoints = false)
        {
            _factory = BuildCollisionFactory();
            Collision = CreateCollision(_factory);
            //_collisionVisualizator.Show(Collision);


            var mEPCollision = Collision as MEPCollision;

            //build Graph
            var graph = GetGraph(_doc, mEPCollision);

            if (insertAxiliaryPoints)
            {

                InsertPoints(graph, mEPCollision,
                _doc, _traceSettings, _collisionDetector);
            }

            return GetProcessor();

            _trfAuto.CreateAsync(() => { _graphVisualisator.Show(graph); }, "ShowGraph");
            //return null;

            ResolveProcessor<IMEPCollision, IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> GetProcessor()
            {
                //Create and config pathFind factory
               
                var pathFindFactory = new XYZVertexPathFinderFactory(_uiDoc)
                {
                    TraceSettings = _traceSettings,
                };
                var pathFinder = pathFindFactory.GetInstance();
                pathFinder.AccountInitialDirections = true;
                pathFinder.MinimizePathNodes = true;
                pathFinder.AllowSecondElementForBasis = true;
                pathFinder.MaxTime = 1000000;

                var f1 = new PathFindFactoryBuilder(_uiDoc, _collisionDetector, graph, pathFinder)
                {
                    AutoTasks = true,
                    IterationCategories = GetIterationCategories(),
                    Logger = _logger,
                    TaskVisualizator = _taskVisualizator,
                    ResultVisualizator = _graphVisualisator
                }.WithCollision(mEPCollision).Create();               

                //add factories
                var factories = new List<IResolveFactory<IMEPCollision, IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>>>
            {
                f1
            };

                return
                    new ResolveProcessor<IMEPCollision, IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>>(factories)
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


        public void InsertPoints(AdjacencyGraph<IVertex, Edge<IVertex>> graph, MEPCollision mEPCollision,
            Document doc, ITraceSettings traceSettings, IElementCollisionDetector collisionDetector)
        {
            var mEPCurve = mEPCollision.Item1Model.MEPCurve;

            var root = graph.Roots().FirstOrDefault();
            var outEdges = graph.OutEdges(root).ToList();
            if (outEdges is null || outEdges.Count() != 2) { return; }

            var segementFactory = GetFactory(doc, collisionDetector,
                true, traceSettings, mEPCollision);
            //{ return null; }
            var p1 = GetClosesetPoint(segementFactory, outEdges[0]);
            var p2 = GetClosesetPoint(segementFactory, outEdges[1]);

            if (p1 != null && graph.TryInsert(mEPCurve, p1))
            { Log.Information($"Closest point {p1} was inserted to graph successfully."); }
            if (p2 != null && graph.TryInsert(mEPCurve, p2))
            { Log.Information($"Closest point {p2} was inserted to graph successfully."); }

            XYZ GetClosesetPoint(SegmentFactory segmentFactory, IEdge<IVertex> edge)
            {
                var segments = segmentFactory.GetFreeSegments(edge).ToList();
                var firstSegment = segments.FirstOrDefault();

                return firstSegment.Equals(default) ?
                    null :
                    firstSegment.From.ToXYZ();
            }
        }

        public SegmentFactory GetFactory(Document doc, IElementCollisionDetector collisionDetector, bool insulationAccount,
           ITraceSettings traceSettings, MEPCollision mEPCollision)
        {
            var model = mEPCollision.Item1Model;
            var mEPCurveSize = Math.Min(model.Width, model.Height);
            var insulationThickness = insulationAccount
                ? model.InsulationThickness
                : 0;

            var minDistanceToElements = mEPCurveSize / 2 + insulationThickness + traceSettings.C;
            var minDistanceToConnector = traceSettings.D + model.ElbowRadius;
            var minDistanceFromSource = (traceSettings.D + 2 * model.ElbowRadius) / Math.Tan(traceSettings.A.DegToRad());

            var segementFactory = new SegmentFactory(doc, collisionDetector)
            {
                MinDistanceToElements = minDistanceToElements,
                MinDistanceToConnector = minDistanceToConnector,
                MinDistanceFromSource = minDistanceFromSource
            };

            return segementFactory;
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
    }
}
