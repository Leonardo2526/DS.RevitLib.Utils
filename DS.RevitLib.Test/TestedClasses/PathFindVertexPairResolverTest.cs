using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Resolvers.TaskCreators;
using DS.GraphUtils.Entities;
using DS.RevitCollisions;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Graphs;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using DS.RevitLib.Utils.PathCreators.AlgorithmBuilder;
using DS.RevitLib.Utils.PathCreators;
using DS.RevitLib.Utils.PathCreators.AlgorithmVertexBuilder;
using DS.RevitLib.Utils.Resolve.TaskCreators;
using DS.RevitLib.Utils.Various;
using QuickGraph;
using Rhino.Geometry;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using DS.ClassLib.VarUtils.GridMap;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitCollisions.Resolve.TaskResolvers;
using DS.RevitCollisions.Resolve.TaskCreators;

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

            _graphVisualisator = new GraphVisulisator(uiDoc)
            {
                ShowElementIds = false,
                ShowVerticesIds = false,
                ShowDirecionts = true,
                TransactionFactory = _trfAuto,
                RefreshView = true
            };
        }

        public void RunCase1()
        {
            MEPCurve baseMEPCurve;
            try
            {
                baseMEPCurve = new MEPCurveSelector(_uiDoc) { AllowLink = false }.
                    Pick("Выберите базовый линейный элемент внутри удаляемого диапазона.");
            }
            catch (Exception)
            { return; }

            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> sourceGraph = null;
            AdjacencyGraph<IVertex, Edge<IVertex>> targetGraph = null;
            if (_deleteInsideElements)
            {
                //Create graph
                sourceGraph = CreateGraphByMEPCurve(baseMEPCurve);
                targetGraph = sourceGraph as AdjacencyGraph<IVertex, Edge<IVertex>>;
                targetGraph = targetGraph.Clone();
            }

            if (sourceGraph == null)
            {
                FindPath(baseMEPCurve);
            }
            else
            {
                FindPathWithGraph(baseMEPCurve, sourceGraph, targetGraph);
            }


        }

        private void FindPathWithGraph(MEPCurve baseMEPCurve,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> sourceGraph,
            AdjacencyGraph<IVertex, Edge<IVertex>> targetGraph)
        {
            //build task
            var taskCreatorFactory = new ManualTaskCreatorFactory(_uiDoc, targetGraph, _collisionDetector)
            {
                AvailableCategories = GetIterationCategories(),
                ExternalOutline = null,
                InsulationAccount = true,
                TraceSettings = _traceSettings,
                Messenger = new TaskDialogMessenger(),
                Logger = _logger,
                BaseMEPCurve = baseMEPCurve,
            };

            var taskCreator = taskCreatorFactory.Create();
            var task = taskCreator.CreateTask(null);

            if (targetGraph != null)
            {
                targetGraph.PrintEdges();
                targetGraph.PrintEdgesVerticesTags();
            }

            _taskVisualizator.Show(task);
            _uiDoc.RefreshActiveView();

            //build resolver
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

            var resolver = new PathFindGraphResolver(pathFinder, _doc,
                _collisionDetector, targetGraph, baseMEPCurve, baseMEPCurve)
            {
                Logger = _logger
            };

            //find path
            var result = resolver.TryResolve(task);
            _graphVisualisator.Show(result);
        }

        private void FindPath(MEPCurve baseMEPCurve)
        {
            //build task
            var taskCreatorFactory = new ManualXYZElementTaskCreatorFactory(_uiDoc, _collisionDetector)
            {
                AvailableCategories = GetIterationCategories(),
                ExternalOutline = null,
                InsulationAccount = true,
                TraceSettings = _traceSettings,
                Messenger = new TaskDialogMessenger(),
                Logger = _logger,
                BaseMEPCurve = baseMEPCurve,
            };

            var taskCreator = taskCreatorFactory.Create();
            var task = taskCreator.CreateTask(null);

            task.Item1.Item2.Show(_doc, 0, _trfAuto);
            task.Item2.Item2.Show(_doc, 0, _trfAuto);
            _uiDoc.RefreshActiveView();

            //build resolver
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

            var resolver =
                new PathFindResolver(pathFinder, _doc, _collisionDetector, baseMEPCurve, baseMEPCurve, null)
                {
                    Logger = _logger,
                };
            var c1 = new ConnectionPoint(task.Item1.Item1, task.Item1.Item2);
            var c2 = new ConnectionPoint(task.Item2.Item1, task.Item2.Item2);

            //find path
            var graph = resolver.TryResolve((c1, c2));
            _graphVisualisator.Show(graph);
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

        private IVertex TryAddVertex(IVertex vertexToAdd, IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph)
        {
            var vertex = vertexToAdd is TaggedGVertex<(int, Point3d)> taggedIntPointVertex ?
                taggedIntPointVertex.ToVertexPoint() :
                vertexToAdd;

            if (!vertex.TryFindTaggedVertex(graph, out var foundVertex))
            {
                var aGraph = graph as AdjacencyGraph<IVertex, Edge<IVertex>>;
                var location = vertex.GetLocation(_doc).ToPoint3d();
                if (aGraph.TryInsert(location, _doc))
                { foundVertex = aGraph.Vertices.Last(); }
            }

            return foundVertex;
        }
    }
}

