using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Resolvers;
using DS.GraphUtils.Entities;
using DS.RevitCollisions.Resolve.ResolveFactories;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using DS.RevitLib.Utils.PathCreators;
using DS.RevitLib.Utils.PathCreators.AlgorithmVertexBuilder;
using DS.RevitLib.Utils.Various;
using QuickGraph;
using Rhino.Geometry;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class PathFindVertexPairResolverTest
    {
        private readonly bool _deleteInsideElements = true;
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trfIn;
        private readonly ContextTransactionFactory _trfOut;
        private readonly ContextTransactionFactory _trfAuto;
        private readonly GraphVisulisator _graphVisualisator;
        private readonly ElementCollisionDetector _collisionDetector;
        private readonly TraceSettings _traceSettings;
        private readonly Logger _logger;
        private readonly VertexPairVisualizator _taskVisualizator;
        private readonly ElementPointPairVisualizator _xYZTaskVisualizator;

        public PathFindVertexPairResolverTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;

            _trfIn = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
            _trfOut = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Outside);
            _trfAuto = new ContextTransactionFactory(_doc);

            _collisionDetector = new ElementCollisionDetector(_doc, new ElementIntersectionFactory(_doc));

            _traceSettings = new TraceSettings();

            _logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .CreateLogger();

            _taskVisualizator = new VertexPairVisualizator(_uiDoc)
            {
                TransactionFactory = _trfAuto,
                RefreshView = true,
            };

            _xYZTaskVisualizator = new ElementPointPairVisualizator(_uiDoc)
            {
                TransactionFactory = _trfAuto,
                RefreshView = true,
            };

            _graphVisualisator = new GraphVisulisator(uiDoc)
            {
                ShowElementIds = false,
                ShowVerticesIds = false,
                ShowDirecionts = true,
                TransactionFactory = _trfAuto,
                RefreshView = true
            };
        }


        public void RunTest1()
        {
            var processor = CreateProcessor();
            var result = processor?.TryResolve();
        }

        public ResolveProcessor<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> CreateProcessor()
        {
            MEPCurve baseMEPCurve;
            try
            {
                baseMEPCurve = new MEPCurveSelector(_uiDoc) { AllowLink = false }.
                    Pick("Выберите базовый линейный элемент внутри удаляемого диапазона.");
            }
            catch (Exception)
            { return null; }

            //build Graph
            var sourceGraph = _deleteInsideElements ? CreateGraphByMEPCurve(baseMEPCurve) : null;

            //create and config pathFind factory
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

            //get processor
            var resolveFactory = GetResolveFactory(sourceGraph, pathFinder, baseMEPCurve);
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

        private IResolveFactory<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> GetResolveFactory
            (IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> sourceGraph, XYZPathFinder pathFinder, MEPCurve baseMEPCurve)
        {
            IResolveFactory<IVertexAndEdgeListGraph<IVertex, Edge<IVertex>>> resolveFactory;

            if (sourceGraph == null)
            {
                resolveFactory = new PathFindPointElementFactoryBuilder(_uiDoc, _collisionDetector, pathFinder,
                   baseMEPCurve, baseMEPCurve)
                {
                    TraceSettings = _traceSettings,
                    IterationCategories = GetIterationCategories(),
                    Logger = _logger,
                    TaskVisualizator = _xYZTaskVisualizator,
                    ResultVisualizator = _graphVisualisator,
                    ResolveParallel = true,
                    Messenger = new TaskDialogMessenger(),
                    TransactionFactory = _trfAuto
                }.Create();
            }
            else
            {
                resolveFactory = new PathFindGraphFactoryBuilder(_uiDoc, _collisionDetector, sourceGraph, pathFinder,
                     baseMEPCurve, baseMEPCurve)
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

            return resolveFactory;
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


        private IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> CreateGraphByVertex(IVertex rootVertex,
            MEPCurve mEPCurve)
        {
            var accessoryPartTypes = new List<PartType>() { PartType.Undefined };
            var stopCategories = new Dictionary<BuiltInCategory, List<PartType>>()
            {
                { BuiltInCategory.OST_MechanicalEquipment, accessoryPartTypes }
            };

            var maxLength = 100000.MMToFeet();
            var maxCount = 100;

            var builder = new MEPGraphBuilder(_doc)
            {
                //StopTypes = null,
                StopCategories = stopCategories,
                StopElementRelation = Relation.Parent,
                MaxLength = maxLength,
                MaxVerticesCount = maxCount,
                BoundOutline = null,
                ExcludedTypes = null,
                //ExculdedCategories = exculdedCategories,
            };


            switch (rootVertex)
            {
                case TaggedGVertex<int> taggedGIntVertex:
                    {
                        var elem = _doc.GetElement(new ElementId(taggedGIntVertex.Tag));
                        return builder.Create(elem);
                    }
                case TaggedGVertex<Point3d> taggedPointVertex:
                    {
                        var point = taggedPointVertex.Tag.ToXYZ();
                        return builder.Create(mEPCurve, point);
                    }
                case TaggedGVertex<(int, Point3d)> taggedIntPointVertex:
                    {
                        var point = taggedIntPointVertex.Tag.Item2.ToXYZ();
                        return builder.Create(mEPCurve, point);
                    }
                default:
                    break;
            }

            return null;
        }

        private IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> CreateGraphByMEPCurve(MEPCurve mEPCurve)
        {
            var accessoryPartTypes = new List<PartType>() { PartType.Undefined };
            var stopCategories = new Dictionary<BuiltInCategory, List<PartType>>()
            {
                { BuiltInCategory.OST_MechanicalEquipment, accessoryPartTypes }
            };

            var maxLength = 100000.MMToFeet();
            var maxCount = 100;

            var builder = new MEPGraphBuilder(_doc)
            {
                //StopTypes = null,
                StopCategories = stopCategories,
                StopElementRelation = Relation.Parent,
                MaxLength = maxLength,
                MaxVerticesCount = maxCount,
                BoundOutline = null,
                ExcludedTypes = null,
                //ExculdedCategories = exculdedCategories,
            };

            var point = mEPCurve.GetCenterPoint();
            return builder.Create(mEPCurve, point);
        }
    }
}

