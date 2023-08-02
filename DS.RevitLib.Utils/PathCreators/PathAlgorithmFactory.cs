using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Directions;
using DS.ClassLib.VarUtils.Enumerables;
using DS.ClassLib.VarUtils.Points;
using DS.PathFinder;
using DS.PathFinder.Algorithms.AStar;
using DS.RevitLib.Utils.Bases;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry.Points;
using DS.RevitLib.Utils.MEP;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Plane = Rhino.Geometry.Plane;
using Transform = Rhino.Geometry.Transform;

namespace DS.RevitLib.Utils.PathCreators
{
    /// <summary>
    /// Factory to create a new path find algorythm.
    /// </summary>
    public class PathAlgorithmFactory : IAlgorithmFactory
    {
        #region SettingsFields

        /// <summary>
        /// Data store tolerance.
        /// </summary>
        private readonly int _tolerance = 5;

        /// <summary>
        /// Compound numbers tolerance.
        /// </summary>
        private int _cTolerance = 2;

        private readonly int _mHEstimate = 10;
        private readonly HeuristicFormula _heuristicFormula = HeuristicFormula.Manhattan;
        private readonly bool _mCompactPath = false;
        private readonly bool _punishChangeDirection = true;

        #endregion

        private readonly UIDocument _uiDoc;
        private readonly IBasisStrategy _basisStrategy;
        private readonly List<Element> _docElements;
        private readonly Dictionary<RevitLinkInstance, List<Element>> _linkElementsDict;
        private readonly Document _doc;

        private readonly (Vector3d basisX, Vector3d basisY, Vector3d basisZ) _initialBasis =
            (Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis);

        private (Vector3d basisX, Vector3d basisY, Vector3d basisZ) _pathFindBasis;
        private XYZ _startPoint;
        private XYZ _endPoint;
        private double _step;
        private List<Element> _objectsToExclude;
        private MEPCurve _baseMEPCurve;
        private ITraceSettings _traceSettings;
        private NodeBuilder _nodeBuilder;
        private AStarAlgorithmCDF _algorithm;
        private List<Plane> _planes;

        /// <summary>
        /// Instansiate a factory to create a new path find algorythm.
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <param name="basisStrategy"></param>
        /// <param name="docElements"></param>
        /// <param name="linkElementsDict"></param>
        /// <param name="traceSettings"></param>
        public PathAlgorithmFactory(UIDocument uiDoc, IBasisStrategy basisStrategy, ITraceSettings traceSettings,
            List<Element> docElements, Dictionary<RevitLinkInstance, List<Element>> linkElementsDict = null)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _basisStrategy = basisStrategy;
            _docElements = docElements;
            _linkElementsDict = linkElementsDict;
            _traceSettings = traceSettings;
        }

        #region Properties

        /// <inheritdoc/>
        public IPathFindAlgorithm<Point3d> Algorithm { get => _algorithm; }

        /// <summary>
        /// Start point in UCS.
        /// </summary>
        public Point3d StartPoint { get; private set; }

        /// <summary>
        /// End point in UCS.
        /// </summary>
        public Point3d EndPoint { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IPoint3dConverter PointConverter { get; private set; }


        #endregion

        /// <summary>
        /// Build with some additional paramters.
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="step"></param>
        /// <param name="objectsToExclude"></param>
        /// <param name="allowStartDirection"></param>
        /// <param name="planeTypes"></param>
        /// <returns></returns>
        public PathAlgorithmFactory Build(MEPCurve baseMEPCurve, XYZ startPoint, XYZ endPoint, List<Element> objectsToExclude, List<PlaneType> planeTypes = null)
        {
            _baseMEPCurve = baseMEPCurve;
            _startPoint = startPoint;
            _endPoint = endPoint;
            _objectsToExclude = objectsToExclude;
            _planes = ConvertPlaneTypes(planeTypes);
            Create();

            return this;
        }

        /// <inheritdoc/>
        public void Reset(double step)
        {
            _step = step;
            _nodeBuilder = _nodeBuilder.WithStep(_step);
            _algorithm = _algorithm.WithNodeBuilder(_nodeBuilder);
            _algorithm.ResetToken();
        }

        /// <summary>
        /// Build algorithm with <paramref name="startMEPCurve"/> and <paramref name="endMEPCurve"/> directions to use it by path finding.
        /// </summary>
        /// <param name="startMEPCurve"></param>
        /// <param name="endMEPCurve"></param>
        public void WithInitialDirections(MEPCurve startMEPCurve, MEPCurve endMEPCurve)
        {
            var sp = new Point3d(_startPoint.X, _startPoint.Y, _startPoint.Z);
            var ep = new Point3d(_endPoint.X, _endPoint.Y, _endPoint.Z);

            var startDir = GetDirection(startMEPCurve, sp, ep, out Point3d startANP);
            var endDir = GetDirection(endMEPCurve, ep, sp, out Point3d endANP);
            endDir = Vector3d.Negate(endDir);

            var allConnected = ConnectorUtils.GetAllConnectedElements(startMEPCurve, _doc);
            if (allConnected.Select(obj => obj.Id).Contains(endMEPCurve.Id))
            { startDir = Vector3d.Negate(startDir); }

            _algorithm.StartDirection = startDir;
            _algorithm.StartANP = startANP;
            _algorithm.EndDirection = endDir;
            _algorithm.EndANP = endANP;
        }

        /// <summary>
        /// Create a new algorythm.
        /// </summary>
        /// <returns>
        /// Algorythm to find path between <see cref="StartPoint"/> and  <see cref="EndPoint"/>.
        /// </returns>
        private IPathFindAlgorithm<Point3d> Create()
        {
            var (basisX, basisY, basisZ) = _basisStrategy.GetBasis();

            //specify basis.
            _pathFindBasis = XYZUtils.ToBasis3d(basisX, basisY, basisZ);

            var (transform, inverseTransform) = GetTransforms(
                _initialBasis.basisX, _initialBasis.basisY, _initialBasis.basisZ,
            _pathFindBasis.basisX, _pathFindBasis.basisY, _pathFindBasis.basisZ);

            _pathFindBasis.basisX = _pathFindBasis.basisX.Round(_tolerance);
            _pathFindBasis.basisY = _pathFindBasis.basisY.Round(_tolerance);
            _pathFindBasis.basisZ = _pathFindBasis.basisZ.Round(_tolerance);

            var orths = new List<Vector3d>() { _initialBasis.basisX, _initialBasis.basisY, _initialBasis.basisZ };
            var mainBasis = _pathFindBasis.basisX + _pathFindBasis.basisY + _pathFindBasis.basisZ;

            PointConverter = new Point3dConverter(transform, inverseTransform);

            //convert start and end points.
            var sp = new Point3d(_startPoint.X, _startPoint.Y, _startPoint.Z);
            StartPoint = PointConverter.ConvertToUCS2(sp).Round(_tolerance);
            var ep = new Point3d(_endPoint.X, _endPoint.Y, _endPoint.Z);
            EndPoint = PointConverter.ConvertToUCS2(ep).Round(_tolerance);

            IDirectionFactory directionFactory = new UserDirectionFactory();
            directionFactory.Build(_initialBasis.basisX, _initialBasis.basisY, _initialBasis.basisZ, _traceSettings.AList);

            IPointVisualisator<Point3d> pointVisualisator =
                new Point3dVisualisator(_uiDoc, PointConverter, 50.MMToFeet(), null, true);

            _nodeBuilder = new NodeBuilder(
                _heuristicFormula, _mHEstimate, StartPoint, EndPoint,
                _step, orths, _mCompactPath, _punishChangeDirection)
            {
                Tolerance = _tolerance,
                PointVisualisator = pointVisualisator
                //CTolerance = _cTolerance
            };

            ITraceCollisionDetector<Point3d> collisionDetector =
                new CollisionDetectorByTrace(_doc, _baseMEPCurve, _traceSettings, _docElements, _linkElementsDict, PointConverter)
                {
                    ObjectsToExclude = _objectsToExclude,
                    OffsetOnEndPoint = true
                };

            IRefineFactory<Point3d> refineFactory = new PathRefineFactory();

            var dirIterator = new DirectionIterator(_planes, _traceSettings.AList);

            //find restrict area
            Vector3d boundMoveVector = GetMoveVector();
            var (minPoint, maxPoint) = PointsUtils.GetBound(StartPoint, EndPoint, boundMoveVector);
            _algorithm = new AStarAlgorithmCDF(_traceSettings, _nodeBuilder, dirIterator, collisionDetector, refineFactory)
            {
                Tolerance = _tolerance,
                CTolerance = _cTolerance,
                //TokenSource = new CancellationTokenSource(),
                TokenSource = new CancellationTokenSource(5000),
                PointVisualisator = pointVisualisator
            }
            .WithBounds(minPoint, maxPoint);

            return Algorithm;
        }

        private Vector3d GetDirection(MEPCurve mEPCurve, Point3d pointOnMEPCurve, Point3d pointOnSecondMEPCurve, out Point3d aNP)
        {
            Vector3d direction;

            var cons = ConnectorUtils.GetConnectors(mEPCurve);
            cons = cons.OrderBy(c => c.Origin.ToPoint3d().DistanceTo(pointOnMEPCurve)).ToList();
            aNP = cons[1].Origin.ToPoint3d();
            aNP = PointConverter.ConvertToUCS2(aNP).Round(_tolerance);

            var spUCS2 = PointConverter.ConvertToUCS2(pointOnMEPCurve);
            var epUCS2 = PointConverter.ConvertToUCS2(pointOnSecondMEPCurve);
            direction = spUCS2 - aNP;
            direction = Vector3d.Divide(direction, direction.Length).Round(_tolerance);

            if ((aNP - epUCS2).Length < _cTolerance)
            {
                direction = Vector3d.Negate(direction);
                aNP = Point3d.Origin;
            }

            return direction;
        }

        private List<Plane> ConvertPlaneTypes(List<PlaneType> planeTypes)
        {
            var planes = new List<Plane>();

            PlaneType xyPlaneType = planeTypes.FirstOrDefault(p => p == PlaneType.XY);
            PlaneType xzPlaneType = planeTypes.FirstOrDefault(p => p == PlaneType.XZ);
            PlaneType yzPlaneType = planeTypes.FirstOrDefault(p => p == PlaneType.YZ);

            var xyPlane = Plane.WorldXY;
            var xzPlane = Plane.WorldZX;
            var yzPlane = Plane.WorldYZ;

            if (xyPlaneType != default) { planes.Add(xyPlane); }
            if (xzPlaneType != default) { planes.Add(xzPlane); }
            if (yzPlaneType != default) { planes.Add(yzPlane); }

            //default planes set
            if (planes.Count == 0)
            {
                planes.Add(xyPlane);
                planes.Add(xzPlane);
                planes.Add(yzPlane);
            }

            return planes;
        }

        private Vector3d GetMoveVector()
        {
            var basis = _initialBasis;

            double offsetX = 5000.MMToFeet();
            double offsetY = 5000.MMToFeet();
            double offsetZ = 5000.MMToFeet();

            return new Vector3d(basis.basisX.X * offsetX, basis.basisY.Y * offsetY, basis.basisZ.Z * offsetZ);
        }

        private (Transform transform, Transform inversedTransform) GetTransforms(
            Vector3d initialBasisX, Vector3d initialBasisY, Vector3d initialBasisZ,
            Vector3d finalBasisX, Vector3d finalBasisY, Vector3d finalBasisZ
            )
        {
            bool initialRightHanded = Vector3d.AreRighthanded(initialBasisX, initialBasisY, initialBasisZ);
            bool finalRightHanded = Vector3d.AreRighthanded(finalBasisX, finalBasisY, finalBasisZ);
            bool orthonormal = Vector3d.AreOrthonormal(finalBasisX, finalBasisY, finalBasisZ);

            if (!finalRightHanded)
            {
                finalBasisZ = Vector3d.Negate(finalBasisZ);
                _pathFindBasis.basisZ = finalBasisZ;
            }
            finalRightHanded = Vector3d.AreRighthanded(finalBasisX, finalBasisY, finalBasisZ);
            orthonormal = Vector3d.AreOrthonormal(finalBasisX, finalBasisY, finalBasisZ);

            Transform transform = Transform.ChangeBasis(
                initialBasisX, initialBasisY, initialBasisZ,
                finalBasisX, finalBasisY, finalBasisZ);
            transform.GetEulerZYZ(out double alpha1, out double beta1, out double gamma1);

            transform.TryGetInverse(out Transform inverseTransform);

            double alpha = alpha1.RadToDeg();
            double beta = beta1.RadToDeg();
            double gamma = gamma1.RadToDeg();

            return (transform, inverseTransform);
        }
    }
}
