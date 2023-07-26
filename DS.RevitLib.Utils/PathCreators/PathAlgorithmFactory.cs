using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Directions;
using DS.ClassLib.VarUtils.Points;
using DS.PathFinder;
using DS.RevitLib.Utils.Bases;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry.Points;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Solids.Models;
using FrancoGustavo.Algorithm;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Transform = Rhino.Geometry.Transform;

namespace DS.RevitLib.Utils.PathCreators
{
    /// <summary>
    /// Factory to create a new path find algorythm.
    /// </summary>
    public class PathAlgorithmFactory
    {
        private readonly UIDocument _uiDoc;
        private XYZ _startPoint;
        private XYZ _endPoint;
        private readonly IBasisStrategy _basisStrategy;
        private readonly List<Element> _docElements;
        private readonly Dictionary<RevitLinkInstance, List<Element>> _linkElementsDict;
        private double _step;
        private List<Element> _objectsToExclude;
        private readonly Document _doc;
        private readonly int _tolerance = 3;
        private readonly (Vector3d basisX, Vector3d basisY, Vector3d basisZ) _initialBasis =
            XYZUtils.ToBasis3d(XYZ.BasisX, XYZ.BasisY, XYZ.BasisZ);
        private (Vector3d basisX, Vector3d basisY, Vector3d basisZ) _pathFindBasis;

        private ITraceSettings _traceSettings;

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

        /// <summary>
        /// Build with some additional paramters.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="step"></param>
        /// <param name="objectsToExclude"></param>
        /// <returns></returns>
        public PathAlgorithmFactory Build(XYZ startPoint, XYZ endPoint, double step, List<Element> objectsToExclude)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _step = step;
            _objectsToExclude = objectsToExclude;

            return this;
        }

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

        /// <summary>
        /// Create a new algorythm.
        /// </summary>
        /// <returns>
        /// Algorythm to find path between <see cref="StartPoint"/> and  <see cref="EndPoint"/>.
        /// </returns>
        public IPathFindAlgorithm<Point3d> Create()
        {
            var (basisX, basisY, basisZ) = _basisStrategy.GetBasis();

            MEPCurve basisMEPCurve1 = null;
            if (_basisStrategy is TwoMEPCurvesBasisStrategy twoMC)
            { basisMEPCurve1 = twoMC.MEPCurve1; var basisMEPCurve2 = twoMC.MEPCurve2; }

            //specify basis.
            _pathFindBasis = XYZUtils.ToBasis3d(basisX, basisY, basisZ);

            var orths = new List<Vector3d>() { _initialBasis.basisX, _initialBasis.basisY, _initialBasis.basisZ };
            var mainBasis = _pathFindBasis.basisX + _pathFindBasis.basisY + _pathFindBasis.basisZ; 

            var (transform, inverseTransform) = GetTransforms(
                _initialBasis.basisX, _initialBasis.basisY, _initialBasis.basisZ,
            _pathFindBasis.basisX, _pathFindBasis.basisY, _pathFindBasis.basisZ);

            //_pathFindBasis.basisX = _pathFindBasis.basisX.Round(_tolerance);
            //_pathFindBasis.basisY = _pathFindBasis.basisY.Round(_tolerance);
            //_pathFindBasis.basisZ = _pathFindBasis.basisZ.Round(_tolerance);

            PointConverter = new Point3dConverter(transform, inverseTransform);

            //convert start and end points.
            var sp = new Point3d(_startPoint.X, _startPoint.Y, _startPoint.Z);
            StartPoint = PointConverter.ConvertToUCS2(sp);
            var ep = new Point3d(_endPoint.X, _endPoint.Y, _endPoint.Z);
            EndPoint = PointConverter.ConvertToUCS2(ep);

            IDirectionFactory directionFactory = new UserDirectionFactory();
            directionFactory.Build(_initialBasis.basisX, _initialBasis.basisY, _initialBasis.basisZ, _traceSettings.AList);

            var mHEstimate = 20;
            //return null;
            INodeBuilder nodeBuilder = new NodeBuilder(
                FrancoGustavo.HeuristicFormula.Manhattan, mHEstimate, StartPoint, EndPoint, _step, orths, false, false);
            ITraceCollisionDetector<Point3d> collisionDetector = 
                new CollisionDetectorByTrace(_doc, basisMEPCurve1, _traceSettings, _docElements, _linkElementsDict, PointConverter)
            { ObjectsToExclude = _objectsToExclude };

        IRefineFactory<Point3d> refineFactory = new PathRefineFactory();
            IPointVisualisator<Point3d> pointVisualisator =
                new Point3dVisualisator(_uiDoc, PointConverter, 100.MMToFeet(), null, true);

            var userDirectionFactory = directionFactory as UserDirectionFactory;
            if (userDirectionFactory == null) { return null; }

            //specify all search directions.
            var dirs1 = userDirectionFactory.Plane1_Directions;
            var dirs2 = userDirectionFactory.Plane2_Directions;
            var dirs3 = userDirectionFactory.Plane3_Directions;
            var alldirs = userDirectionFactory.Directions;

            //find restrict area
            Vector3d boundMoveVector = GetMoveVector();
            var (minPoint, maxPoint) = PointsUtils.GetBound(StartPoint, EndPoint, boundMoveVector);

            var factory = new AStarAlgorithmCDF(_traceSettings, nodeBuilder, directionFactory, collisionDetector, refineFactory)
            {
                TokenSource = new CancellationTokenSource(),
                PointVisualisator = pointVisualisator
            }
            .WithBounds(minPoint, maxPoint)
            .WithSearchDirections(dirs1);


            return factory;
        }

        private Vector3d GetMoveVector()
        {
            var basis = _initialBasis;

            double offsetX = 2000.MMToFeet();
            double offsetY = 2000.MMToFeet();
            double offsetZ = 2000.MMToFeet();

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
