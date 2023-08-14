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
using DS.RevitLib.Utils.Collisions;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Connections;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry.Points;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Various.Bases;
using DS.RevitLib.Utils.Visualisators;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Plane = Rhino.Geometry.Plane;
using Transform = Rhino.Geometry.Transform;

namespace DS.RevitLib.Utils.PathCreators
{
    /// <summary>
    /// Factory to create a new path find algorythm.
    /// </summary>
    internal class PathAlgorithmFactory : IAlgorithmFactory
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
        private ConnectionPoint _startConnectionPoint;
        private ConnectionPoint _endConnectionPoint;
        private Outline _outline;
        private double _step;
        private List<Element> _objectsToExclude;
        private MEPCurve _baseMEPCurve;
        private ITraceSettings _traceSettings;
        private readonly XYZVisualizator _visualisator;
        private IPointVisualisator<Point3d> _pointVisualisator;
        private AStarAlgorithmCDF _algorithm;
        private List<Plane> _planes;
        private INodeBuilder _nodeBuilder;

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
            _visualisator = new XYZVisualizator(uiDoc, 100.MMToFeet(), null, true);
        }

        #region Properties

        /// <inheritdoc/>
        public IPathFindAlgorithm<Point3d, Point3d> Algorithm { get => _algorithm; }

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

        public INodeBuilder NodeBuilder { get => _nodeBuilder; private set => _nodeBuilder = value; }

        private CollisionDetectorByTrace _collisionDetector;

        private static List<PartType> fittingPartTypes = new List<PartType>()
            {
                PartType.Tee,
                   PartType.TapPerpendicular,
                    PartType.TapAdjustable,
                    PartType.SpudPerpendicular,
                    PartType.SpudAdjustable
            };
        private static Dictionary<BuiltInCategory, List<PartType>> stopCategories = new Dictionary<BuiltInCategory, List<PartType>>()
            {
                { BuiltInCategory.OST_DuctFitting, fittingPartTypes },
                { BuiltInCategory.OST_PipeFitting, fittingPartTypes }
            };

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
        public PathAlgorithmFactory Build(MEPCurve baseMEPCurve, ConnectionPoint startPoint, ConnectionPoint endPoint,
            Outline outline, List<Element> objectsToExclude, List<PlaneType> planeTypes = null)
        {
            _baseMEPCurve = baseMEPCurve;
            _startConnectionPoint = startPoint;
            _endConnectionPoint = endPoint;
            _startPoint = startPoint.Point;
            _endPoint = endPoint.Point;
            _outline = outline;
            _objectsToExclude = objectsToExclude;
            _planes = ConvertPlaneTypes(planeTypes);
            Create();

            return this;
        }

        public void Update(int tolerance)
        {
            _algorithm.MToleranceCoef = tolerance;
        }

        /// <summary>
        /// Build algorithm with <paramref name="startMEPCurve"/> and <paramref name="endMEPCurve"/> directions to use it by path finding.
        /// </summary>
        /// <param name="startMEPCurve"></param>
        /// <param name="endMEPCurve"></param>
        public void WithInitialDirections()
        {
            var startDir = GetDirection(_startConnectionPoint, _endConnectionPoint, out Point3d startANP);
            var endDir = GetDirection(_endConnectionPoint, _startConnectionPoint, out Point3d endANP, true);

            if (!_startConnectionPoint.Element.IsCategoryElement(stopCategories))
            {
                _algorithm.StartDirection = startDir;
                _algorithm.StartANP = startANP;
            }

            if (!_endConnectionPoint.Element.IsCategoryElement(stopCategories))
            {
                _algorithm.EndDirection = endDir;
                _algorithm.EndANP = endANP;
            }
        }

        /// <summary>
        /// Create a new algorythm.
        /// </summary>
        /// <returns>
        /// Algorythm to find path between <see cref="StartPoint"/> and  <see cref="EndPoint"/>.
        /// </returns>
        private IPathFindAlgorithm<Point3d, Point3d> Create()
        {
            //specify basis.
            _basisStrategy.GetBasis();
            _pathFindBasis = XYZUtils.ToBasis3d(_basisStrategy.BasisX, _basisStrategy.BasisY, _basisStrategy.BasisZ);
            //basis.ToBasis3d().Show(_uiDoc, 100.MMToFeet(), null, true);

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

            _pointVisualisator =
                new Point3dVisualisator(_uiDoc, PointConverter, 100.MMToFeet(), null, true);

            NodeBuilder = new NodeBuilder(
                _heuristicFormula, StartPoint, EndPoint,
                _step, orths, PointConverter, _mCompactPath, _punishChangeDirection)
            {
                Tolerance = _tolerance,
                PointVisualisator = _pointVisualisator
                //CTolerance = _cTolerance
            };

            _collisionDetector =
                new CollisionDetectorByTrace(_doc, _baseMEPCurve, _traceSettings, _docElements, _linkElementsDict, PointConverter)
                {
                    ObjectsToExclude = _objectsToExclude,
                    OffsetOnEndPoint = false,
                    StartConnectionPoint = _startConnectionPoint,
                    EndConnectionPoint = _endConnectionPoint
                };


            IRefineFactory<Point3d> refineFactory = new PathRefineFactory();

            var dirIterator = new DirectionIterator(_planes, _traceSettings.AList);

            //find restrict area
            //Vector3d boundMoveVector = GetMoveVector();
            //var (minPoint1, maxPoint1) = PointsUtils.GetBound(StartPoint, EndPoint, boundMoveVector);

            var bb = new BoundingBoxXYZ();
            bb.Min = _outline.MinimumPoint;
            bb.Max = _outline.MaximumPoint;
            var points = bb.GetPoints();
            //points.ForEach(p => { p.Show(_doc); });

            var points3d = new List<Point3d>();
            points.ForEach(p => { points3d.Add(p.ToPoint3d()); });
            var pointsUCS2 = new List<Point3d>();
            points3d.ForEach(p => pointsUCS2.Add(PointConverter.ConvertToUCS2(p).Round(_tolerance)));
            (Point3d minPoint, Point3d maxPoint) = PointsUtils.GetMinMax(pointsUCS2);
            //_pointVisualisator.Show(minPoint);
            //_pointVisualisator.Show(maxPoint);
            _algorithm = new AStarAlgorithmCDF(_traceSettings, NodeBuilder, dirIterator, _collisionDetector, refineFactory)
            {
                Tolerance = _tolerance,
                CTolerance = _cTolerance,
                //TokenSource = new CancellationTokenSource(),
                TokenSource = new CancellationTokenSource(5000),
                PointVisualisator = _pointVisualisator,
            }
            .WithBounds(minPoint, maxPoint);

            var dir = MEPCurveUtils.GetDirection(_baseMEPCurve);
            var sourceBasisUCS1 = _baseMEPCurve.GetBasisXYZ(dir, _startPoint).ToBasis3d();
            _algorithm.SourceBasis = PointConverter.ConvertToUCS2(sourceBasisUCS1);
            _collisionDetector.SolidExtractor.SetSource(sourceBasisUCS1);

            return Algorithm;
        }

        public List<Point3d> FindPath()
        {
            _algorithm.ResetToken();
            return _algorithm?.FindPath(StartPoint, EndPoint);
        }

        private Vector3d GetDirection(ConnectionPoint connectionPoint1, ConnectionPoint connectionPoint2, out Point3d aNP, bool inverse = false)
        {
            var mc = connectionPoint1.Element is MEPCurve curve ? curve : connectionPoint1.GetMEPCurve(_objectsToExclude.Select(o => o.Id));
            XYZ dirXYZ = new ConnectionDirectionFactory(connectionPoint1.Point, mc, _uiDoc).
                GetDirection(connectionPoint2.Point, connectionPoint2.Element);
            if (inverse) { dirXYZ = dirXYZ.Negate(); }
            _visualisator.ShowVectorByDirection(connectionPoint1.Point, dirXYZ);

            Point3d dirPoint = dirXYZ.ToPoint3d();
            Point3d dirPointUCS2 = PointConverter.ConvertToUCS2(dirPoint);
            Vector3d dir = dirPointUCS2 - Point3d.Origin;

            var dTolerance = Math.Pow(0.1, _cTolerance);
            var cons = ConnectorUtils.GetConnectors(mc);
            var line = mc.GetCenterLine();
            var conPoints = new List<XYZ>();
            cons.ForEach(c => conPoints.Add(line.Project(c.Origin).XYZPoint));
            conPoints = conPoints.OrderBy(c => c.DistanceTo(connectionPoint1.Point)).
                Where(c => c.DistanceTo(connectionPoint1.Point) > dTolerance).ToList();

            if (mc.Id != connectionPoint1.Element.Id) { conPoints.RemoveAt(0); }
            var checkDir = inverse ? dirXYZ.Negate() : dirXYZ;

            var aTolerance = 3.DegToRad();
            Func<XYZ, bool> func = (c) => Math.Round((connectionPoint1.Point - c).Normalize().AngleTo(checkDir)) == 0;
            var foundCon = conPoints.FirstOrDefault(func);
            if (foundCon == null)
            { aNP = default; }
            else
            {
                aNP = foundCon.ToPoint3d();
                aNP = PointConverter.ConvertToUCS2(aNP).Round(_tolerance);
                _pointVisualisator.Show(aNP);
            }

            return dir;
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
