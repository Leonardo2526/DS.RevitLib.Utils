using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Points;
using DS.GraphUtils.Entities;
using DS.PathFinder;
using DS.PathFinder.Algorithms.Enumeratos;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.PathCreators.AlgorithmBuilder;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.PathCreators
{
    /// <summary>
    /// An object that used to find path between <see cref="Autodesk.Revit.DB.XYZ"/> points.
    /// </summary>
    public class XYZPathFinder : IPathFinder<ConnectionPoint, XYZ>
    {
        private readonly UIDocument _uIDoc;
        private readonly ITraceSettings _traceSettings;
        private readonly Document _doc;
        private readonly PathAlgorithmBuilder _pathAlgorithmBuilder;
        private ISpecifyConnectionPointBoundaries _algorithmBuilder;
        private IElementCollisionDetector _collisionDetector;
        private List<XYZ> _path = new List<XYZ>();
        private ITransactionFactory _transactionFactory;

        /// <summary>
        /// Instantiate an object to find path between <see cref="Autodesk.Revit.DB.XYZ"/> points.
        /// </summary>
        public XYZPathFinder(UIDocument uIDocument, ITraceSettings traceSettings,
            IAlgorithmBuilder algorithmBuilder)
        {
            _uIDoc = uIDocument;
            _traceSettings = traceSettings;
            _doc = _uIDoc.Document;
            if (algorithmBuilder is PathAlgorithmBuilder pathAlgorithmBuilder)
            { _pathAlgorithmBuilder = pathAlgorithmBuilder; }
            else
            {
                throw new TypeAccessException(
                    $"algorithmFactory is not {typeof(PathAlgorithmBuilder)} type");
            }
        }

        #region Properties

        /// <inheritdoc/>
        public List<XYZ> Path { get => _path; }

        /// <summary>
        /// Token to cancel finding path operation.
        /// </summary>
        public CancellationTokenSource ExternalToken { get; set; }

        /// <summary>
        /// Specifies whether allow insulation collisions on resolving or not.
        /// </summary>
        public bool InsulationAccount { get; set; }

        /// <summary>
        /// Factory to create bounds to find path.
        /// </summary>
        public IOutlineFactory OutlineFactory { get; set; }

        /// <summary>
        /// External bound <see cref="Autodesk.Revit.DB.Outline"/>.
        /// <para>
        /// If specified pathFinder will not be able to create bound less or more than this.
        /// </para>
        /// </summary>
        public Outline ExternalOutline { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<BuiltInCategory> ExludedCathegories { get; set; }

        /// <summary>
        /// Specifies if account initial connection elements directios to perform correct connection.
        /// </summary>
        public bool AllowSecondElementForBasis { get; set; }

        /// <summary>
        /// Specifies if account initial connection elements directios to perform correct connection.
        /// </summary>
        public bool AccountInitialDirections { get; set; }

        /// <summary>
        /// Specifies if minimize nodes of path.
        /// </summary>
        public bool MinimizePathNodes { get; set; }

        public ITransactionFactory TransactionFactory
        {
            get
            {
                return _transactionFactory ??=
                    new ContextTransactionFactory(_doc, RevitContextOption.Auto);
            }
            set => _transactionFactory = value;
        }

        public IElementsExtractor ElementsExtractor { get; set; }

        public IWindowMessenger Messenger { get; set; }

        /// <summary>
        /// Specifies if it was failed to exit from startPoint.
        /// </summary>
        public bool IsFailedOnStart { get; private set; }

        /// <summary>
        /// Maximum search time in milliseconds.
        /// </summary>
        public int MaxTime { get; set; }

        public IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> Graph { get; set; }

        #endregion

        /// <summary>
        /// Build path finder.
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <param name="basisMEPCurve1"></param>
        /// <param name="basisMEPCurve2"></param>
        /// <param name="objectsToExclude"></param>
        /// <param name="collisionDetector"></param>
        public void Build(
            MEPCurve baseMEPCurve, 
            MEPCurve basisMEPCurve1, 
            MEPCurve basisMEPCurve2, 
            List<Element> objectsToExclude, IElementCollisionDetector collisionDetector)
        {
            _pathAlgorithmBuilder.TransactionFactory = _transactionFactory;
            _pathAlgorithmBuilder.InsulationAccount = InsulationAccount;
            _algorithmBuilder = _pathAlgorithmBuilder.
                SetBasis(baseMEPCurve, basisMEPCurve1, basisMEPCurve2, AllowSecondElementForBasis).
                SetExclusions(objectsToExclude, ExludedCathegories).
                SetExternalToken(ExternalToken);
            
            _pathAlgorithmBuilder.NextPointStrategy = new NextConnectionPointStrategy(_doc)
            {
                Graph = Graph
            };

            _collisionDetector = collisionDetector;
        }

        /// <inheritdoc/>
        public List<XYZ> FindPath(ConnectionPoint startPoint, ConnectionPoint endPoint)
        {
            var algorithm = _algorithmBuilder.
                 SetBoundaryConditions(
                 startPoint, endPoint, 
                 OutlineFactory, 
                 ExternalOutline, 
                 AccountInitialDirections)?.
                 SetVisualisator().
                 SetDirectionIterator().
                 SetCollisionDetector(_collisionDetector, InsulationAccount, ElementsExtractor).
                 SetNodeBuilder().
                 SetSearchLimit().
                 Build(MinimizePathNodes);

            if (algorithm is null) { return _path; }

            if (MaxTime > 0) { algorithm.MaxTime = MaxTime; }

            PathFindEnumerator pathFindEnumerator =
                GetPathEnumerator(_pathAlgorithmBuilder, _traceSettings, ExternalToken);

            var path = new List<Point3d>();
            while (pathFindEnumerator.MoveNext())
            { path = pathFindEnumerator.Current; }

            if (path == null || path.Count == 0)
            {
                _path = null;
                IsFailedOnStart = algorithm.IsFailedOnStart;               
            }
            else
            { _path = ConvertPath(path, _pathAlgorithmBuilder.PointConverter); }

            return _path;
        }

        private PathFindEnumerator GetPathEnumerator(
            PathAlgorithmBuilder pathAlgorithmBuilder, 
            ITraceSettings traceSettings,
            CancellationTokenSource token)
        {
            var dist = double.MaxValue;
            //var dist = startPoint.Point.DistanceTo(endPoint.Point) / 3;
            var stepEnumerator = new StepEnumerator(
                pathAlgorithmBuilder.NodeBuilder,
                dist.FeetToMM(),
                traceSettings.Step.FeetToMM(),
                true);
            var heuristicEnumerator = new HeuristicEnumerator(
                pathAlgorithmBuilder.NodeBuilder, true);
            var toleranceEnumerator = new ToleranceEnumerator(
                    pathAlgorithmBuilder, 
                    true, 
                    traceSettings.A != 90 && traceSettings.A != 45);
            var pathFindEnumerator = new PathFindEnumerator(
                stepEnumerator, 
                heuristicEnumerator,
                toleranceEnumerator, 
                pathAlgorithmBuilder.Algorithm,
                pathAlgorithmBuilder.StartPoint,
                pathAlgorithmBuilder.EndPoint)
            {
                TokenSource = token
            };
            return pathFindEnumerator;
        }

        /// <inheritdoc/>
        public async Task<List<XYZ>> FindPathAsync(ConnectionPoint startPoint, ConnectionPoint endPoint)
        {
            return await Task.Run(() => FindPath(startPoint, endPoint));
        }

        /// <summary>
        /// Convert <paramref name="path"/> to revit coordinates.      
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pointConverter"></param>
        /// <returns>Converted to <see cref="Autodesk.Revit.DB.XYZ"/> path.</returns>
        private List<XYZ> ConvertPath(List<Point3d> path, IPoint3dConverter pointConverter)
        {

            List<XYZ> pathCoords = new List<XYZ>();

            foreach (var point in path)
            {
                var ucs1Point = pointConverter.ConvertToUCS1(point);
                var xYZ = new XYZ(ucs1Point.X, ucs1Point.Y, ucs1Point.Z);
                pathCoords.Add(xYZ);
            }

            return pathCoords;
        }

    }
}
