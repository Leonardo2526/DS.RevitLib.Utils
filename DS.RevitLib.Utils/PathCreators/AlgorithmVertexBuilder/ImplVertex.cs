using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Enumerables;
using DS.ClassLib.VarUtils.Points;
using DS.GraphUtils.Entities;
using DS.PathFinder;
using DS.PathFinder.Algorithms.AStar;
using DS.RevitLib.Utils.Bases;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.Geometry.Points;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Various.Bases;
using Rhino.Geometry;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Plane = Rhino.Geometry.Plane;
using Transform = Rhino.Geometry.Transform;
using DS.RevitLib.Utils.Graphs;
using QuickGraph;
using Autodesk.Revit.DB.DirectContext3D;

namespace DS.RevitLib.Utils.PathCreators.AlgorithmBuilder
{
    /// <summary>
    /// An object that represents factory to create a new path find algorythm.
    /// </summary>
    public partial class PathAlgorithmVertexBuilder
    {
        private class ImplVertex :
            ISpecifyToken,
            ISpecifyExclusions,
            ISpecifyParameter,
            ISpecifyVertexBoundaries,
            IBuildAlgorithm
        {
            #region SettingsFields

            /// <summary>
            /// Data store tolerance.
            /// </summary>
            private readonly int _tolerance = 5;

            /// <summary>
            /// Compound numbers tolerance.
            /// </summary>
            private readonly int _cTolerance = 2;

            private readonly HeuristicFormula _heuristicFormula = HeuristicFormula.Manhattan;
            private readonly bool _mCompactPath = false;
            private readonly bool _punishChangeDirection = true;

            #endregion

            #region Fields

            private static readonly List<PartType> _fittingPartTypes = new()
            {
                PartType.Tee,
                   PartType.TapPerpendicular,
                    PartType.TapAdjustable,
                    PartType.SpudPerpendicular,
                    PartType.SpudAdjustable
            };
            private static readonly Dictionary<BuiltInCategory, List<PartType>> _stopCategories = new()
            {
                { BuiltInCategory.OST_DuctFitting, _fittingPartTypes },
                { BuiltInCategory.OST_PipeFitting, _fittingPartTypes }
            };

            protected readonly Document _doc;
            private readonly UIDocument _uiDoc;
            private readonly ITraceSettings _traceSettings;
            private readonly PathAlgorithmVertexBuilder _algorithmBuilder;
            private readonly MEPCurve _baseMEPCurve;
            private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;
            private readonly bool _isInsulationAccount;
            private readonly IBasisStrategy _basisStrategy;
            private IPoint3dConverter _pointConverter;
            private List<Element> _objectsToExclude;
            private List<BuiltInCategory> _exludedCathegories;
            private Point3d _startPoint;
            private Point3d _endPoint;
            private readonly ITransactionFactory _transactionFactory;
            private readonly (Vector3d basisX, Vector3d basisY, Vector3d basisZ) _initialBasis =
                (Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis);
            private (Vector3d basisX, Vector3d basisY, Vector3d basisZ) _pathFindBasis;
            private CancellationTokenSource _externalToken;
            private Point3dVisualisator _pointVisualisator;
            private DirectionIterator _dirIterator;
            private CollisionDetectorByTrace _traceCollisionDetector;
            private IVertex _source;
            private IVertex _target;
            private NodeBuilder _nodeBuilder;
            private BasisXYZ _baseMEPCurveBasis;
            private Outline _internalOutline;
            private Point3d _lowerBound;
            private Point3d _upperBound;
            private AStarAlgorithmCDF _algorithm;
            private Vector3d _startDirection;
            private Point3d _startANP;
            private Vector3d _endDirection;
            private Point3d _endANP;

            #endregion

            public ImplVertex(PathAlgorithmVertexBuilder pathAlgorithmBuilder)
            {
                _algorithmBuilder = pathAlgorithmBuilder;
                _algorithm = pathAlgorithmBuilder._algorithm;
                _doc = pathAlgorithmBuilder._doc;
                _uiDoc = pathAlgorithmBuilder._uiDoc;
                _traceSettings = pathAlgorithmBuilder._traceSettings;
                _basisStrategy = pathAlgorithmBuilder._basisStrategy;
                _transactionFactory = pathAlgorithmBuilder.TransactionFactory;
                _baseMEPCurve = pathAlgorithmBuilder._baseMEPCurve;
                _graph = pathAlgorithmBuilder._graph;
                _isInsulationAccount = pathAlgorithmBuilder.IsInsulationAccount;
            }

            #region PublicMethods

            public ISpecifyToken SetExclusions(List<Element> objectsToExclude,
                List<BuiltInCategory> exludedCathegories)
            {
                //add objectsToExclude with its insulations
                var objectToExcludeIds = objectsToExclude.Select(obj => obj.Id).ToList();

                var insulations = new List<Element>();
                foreach (var item in objectsToExclude)
                {
                    InsulationLiningBase ins = null;
                    try
                    {
                        ins = InsulationLiningBase.GetInsulationIds(item.Document, item.Id).
                            Select(x => item.Document.GetElement(x) as InsulationLiningBase).
                            FirstOrDefault();
                    }
                    catch (Exception)
                    { }
                    if (ins is not null && !objectToExcludeIds.Contains(ins.Id))
                    { insulations.Add(_doc.GetElement(ins.Id)); }
                }

                objectsToExclude.AddRange(insulations);
                _objectsToExclude = objectsToExclude;
                _exludedCathegories = exludedCathegories;

                return this;
            }

            public ISpecifyToken SetPointConverter(Point3d startPoint)
            {
                _pathFindBasis = XYZUtils.ToBasis3d(
                    _basisStrategy.BasisX,
                    _basisStrategy.BasisY,
                    _basisStrategy.BasisZ);

                var rotationTransform = GetTransforms(
                    _initialBasis.basisX, _initialBasis.basisY, _initialBasis.basisZ,
                _pathFindBasis.basisX, _pathFindBasis.basisY, _pathFindBasis.basisZ);

                var initialOrigin = Point3d.Origin;
                var pathFindBasisOrigin = startPoint;
                var translationTransform =
                    GetTranslation(initialOrigin, pathFindBasisOrigin);

                _pointConverter = _algorithmBuilder.PointConverter =
                    new Point3dConverter(rotationTransform, translationTransform);

                return this;
            }

            private Transform GetTranslation(Point3d initialOrigin, Point3d pathFindBasisOrigin)
                => Transform.Translation((initialOrigin - pathFindBasisOrigin).Round(_tolerance));

            public ISpecifyVertexBoundaries SetExternalToken2(CancellationTokenSource externalCancellationToken)
            {
                _externalToken = externalCancellationToken;
                return this;
            }

            public ISpecifyParameter SetBoundaryConditions(
                IVertex source, IVertex target,
                IOutlineFactory outlineFactory = null,
                Outline externalOutline = null,
                bool accountInitialDirections = false)
            {
                _source = source;
                _target = target;
                var startPoint = _source.GetLocation(_doc).RoundVector(_tolerance);
                var endPoint = _target.GetLocation(_doc).RoundVector(_tolerance);


                SetPointConverter(startPoint.ToPoint3d());


                var dir = MEPCurveUtils.GetDirection(_baseMEPCurve);
                _baseMEPCurveBasis = _baseMEPCurve.GetBasisXYZ(dir, startPoint);

                _internalOutline = BuildOutline(
                    source,
                    target,
                    externalOutline,
                    outlineFactory);
                if (_internalOutline == null) { return null; }

                //_collisionDetector.ElementsExtractor.Outline = _internalOutline;

                //convert start and end points.
                var sp = new Point3d(startPoint.X, startPoint.Y, startPoint.Z);
                _startPoint = _algorithmBuilder.StartPoint =
                    _pointConverter.ConvertToUCS2(sp).Round(_tolerance);
                var ep = new Point3d(endPoint.X, endPoint.Y, endPoint.Z);
                _endPoint = _algorithmBuilder.EndPoint =
                    _pointConverter.ConvertToUCS2(ep).Round(_tolerance);

                WithInitialDirections();


                void WithInitialDirections()
                {
                    if (!accountInitialDirections)
                    { return; }

                    (_startDirection, _startANP) = GetDirection(source, true);
                    (_endDirection, _endANP) = GetDirection(target);

                    (Vector3d dir, Point3d ANP) GetDirection(IVertex vertex, bool inverse = false)
                    {
                        _graph.TryGetOutEdges(vertex, out var outEdges);
                        var firstEdge = outEdges.FirstOrDefault();
                        if (firstEdge == null)
                        {
                            _graph.ToBidirectionalGraph().TryGetInEdges(vertex, out var inEdges);
                            firstEdge = inEdges.FirstOrDefault();
                        }

                        var mEPCurve = firstEdge.TryGetMEPCurve(_doc) ?? 
                            throw new ArgumentNullException("Failed to find MEPCurve on connection point.");

                        var  dir = firstEdge.Direction(_doc);
                        dir = inverse ? -dir : dir; 

                        (Point3d sourcePoint1, Point3d targetPoint1) = firstEdge.GetConnectorsLocation(_doc);
                        var location = vertex.GetLocation(_doc).ToPoint3d();
                        var ANP = location.DistanceTo(targetPoint1) < 0.001 ? default : targetPoint1;

                        return (dir, ANP);
                    }

                }


                return this;
            }


            public ISpecifyParameter SetCollisionDetector(IElementCollisionDetector collisionDetector, bool insulationAccount,
                IElementsExtractor elementsExtractor)
            {
                elementsExtractor.ExludedCategories = _exludedCathegories;
                elementsExtractor.Outline = _internalOutline;

                collisionDetector.IsInsulationAccount = insulationAccount;
                collisionDetector.ExcludedElements = _objectsToExclude;
                collisionDetector.ExculdedCategories = _exludedCathegories;
                collisionDetector.ActiveDocElements = elementsExtractor.ActiveDocElements;
                collisionDetector.LinkElements = elementsExtractor.LinkElements;

                _traceCollisionDetector =
                    new CollisionDetectorByTrace(_doc,
                    _baseMEPCurve,
                    _traceSettings,
                    insulationAccount, _graph, collisionDetector,
                    _pointConverter, _transactionFactory)
                    {
                        ObjectsToExclude = _objectsToExclude,
                        OffsetOnEndPoint = false,
                        Source = _source,
                        EndConnectionPoint = _target,
                    };
                return this;
            }

            public ISpecifyParameter SetDirectionIterator()
            {
                var dirs = new List<int>() { (int)_traceSettings.A };
                var planes = new List<Plane>() { Plane.WorldXY, Plane.WorldZX, Plane.WorldYZ };
                _dirIterator = new DirectionIterator(planes, dirs);
                return this;
            }

            public ISpecifyParameter SetNodeBuilder()
            {
                var orths = new List<Vector3d>()
                { _initialBasis.basisX, _initialBasis.basisY, _initialBasis.basisZ };

                _nodeBuilder = _algorithmBuilder.NodeBuilder = new NodeBuilder(
               _heuristicFormula, _startPoint, _endPoint,
               _traceSettings.Step, orths, _pointConverter, _traceSettings, _mCompactPath, _punishChangeDirection)
                {
                    Tolerance = _tolerance,
                    PointVisualisator = _pointVisualisator,
                    EndDirection = _endDirection
                    //CTolerance = _cTolerance
                };

                return this;
            }

            public ISpecifyParameter SetVisualisator()
            {
                _pointVisualisator =
                new Point3dVisualisator(_uiDoc, _pointConverter,
                100.MMToFeet(), _transactionFactory, true);

                return this;
            }

            public IBuildAlgorithm SetSearchLimit()
            {
                var bb = new BoundingBoxXYZ();
                bb.Min = _internalOutline.MinimumPoint;
                bb.Max = _internalOutline.MaximumPoint;
                var points = bb.GetPoints();
                //points.ForEach(p => { p.Show(_doc); });
                //_transactionFactory.CreateAsync(() => bb.Show(_doc), "show box");
                //return null;

                var points3d = new List<Point3d>();
                points.ForEach(p => { points3d.Add(p.ToPoint3d()); });
                var pointsUCS2 = new List<Point3d>();
                points3d.ForEach(p => pointsUCS2.Add(_pointConverter.ConvertToUCS2(p).Round(_tolerance)));
                (Point3d minPoint, Point3d maxPoint) = PointsUtils.GetMinMax(pointsUCS2);

                _lowerBound = minPoint;
                _upperBound = maxPoint;

                return this;
            }

            public AStarAlgorithmCDF Build(bool minimizePathNodes = false)
            {
                var sourceBasisUCS1 = _baseMEPCurveBasis.ToBasis3d();
                var sourceBasis = _pointConverter.ConvertToUCS2(sourceBasisUCS1);
                var refineFactory = new PathRefineFactory(_traceSettings, _traceCollisionDetector, sourceBasis)
                {
                    MinNodes = minimizePathNodes
                };

                _algorithm = new AStarAlgorithmCDF(
                    _traceSettings,
                    _nodeBuilder, _dirIterator,
                    _traceCollisionDetector,
                    refineFactory)
                {
                    Tolerance = _tolerance,
                    CTolerance = _cTolerance,
                    PointVisualisator = _pointVisualisator,
                    StartDirection = _startDirection,
                    EndDirection = _endDirection,
                    StartANP = _startANP,
                    EndANP = _endANP,
                    DirectionValidator = _algorithmBuilder._directionValidator,
                    ExternalTokenSource = _externalToken
                }.
                WithBounds(_lowerBound, _upperBound);

                _algorithm.SourceBasis = sourceBasis;
                _traceCollisionDetector.SolidExtractor.SetSource(sourceBasisUCS1);
                _algorithmBuilder._algorithm = _algorithm;
                return _algorithm;
            }

            #endregion

            #region PrivateMethods

            private Outline BuildOutline(
                IVertex source, IVertex target,
                Outline externalOutline, IOutlineFactory outlineFactoryBase)
            {
                outlineFactoryBase ??= new OutlineFactory(_doc);
                if (outlineFactoryBase is not OutlineFactory outlineFactory) { return null; }

                double defaultOffset = 5000.MMToFeet();
                outlineFactory.XOffset = defaultOffset;
                outlineFactory.YOffset = defaultOffset;
                outlineFactory.ZOffset = defaultOffset;
                outlineFactory.IsPointEnableOutside = false;

                var p1 = source.GetLocation(_doc);
                var e1 = source.TryGetFamilyInstance(_doc);

                var p2 = target.GetLocation(_doc);
                var e2 = target.TryGetFamilyInstance(_doc);

                var bounds1 = GetFloorBounds((e1, p1), _doc, _traceSettings.H, _traceSettings.B, _isInsulationAccount);
                var bounds2 = GetFloorBounds((e2, p2), _doc, _traceSettings.H, _traceSettings.B, _isInsulationAccount);

                var outline = outlineFactory.Create((p1, bounds1), (p2, bounds2));
                //outline.Show(_doc, _transactionFactory);
                if (String.IsNullOrEmpty(outlineFactory.ErrorMessage))
                {
                    if (externalOutline is null) { return outline; }
                    else { return outline.GetIntersection(externalOutline); }
                }
                else
                {
                    Debug.WriteLine(outlineFactory.ErrorMessage);
                    _externalToken?.Cancel(); return null;
                }
            }

            private Transform GetTransforms(
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


                double alpha = alpha1.RadToDeg();
                double beta = beta1.RadToDeg();
                double gamma = gamma1.RadToDeg();

                return transform;
            }

            //private Vector3d GetDirection(
            //    IVertex connectionPoint1,
            //    IVertex connectionPoint2,
            //    out Point3d aNP, bool inverse = false)
            //{
            //    var mc = connectionPoint1.Element is MEPCurve curve ? curve : connectionPoint1.GetMEPCurve(_objectsToExclude.Select(o => o.Id));

            //    XYZ dirXYZ = connectionPoint1.Direction ??
            //        connectionPoint1.GetDirection(connectionPoint2.Point, connectionPoint2.Element, _objectsToExclude);

            //    if (inverse) { dirXYZ = dirXYZ.Negate(); }
            //    //_visualisator.ShowVectorByDirection(connectionPoint1.Point, dirXYZ);

            //    Vector3d dir = _pointConverter.ConvertToUCS2(dirXYZ.ToVector3d());

            //    var dTolerance = Math.Pow(0.1, _cTolerance);
            //    var cons = ConnectorUtils.GetConnectors(mc);
            //    var line = mc.GetCenterLine();
            //    var conPoints = new List<XYZ>();
            //    cons.ForEach(c => conPoints.Add(line.Project(c.Origin).XYZPoint));
            //    conPoints = conPoints.OrderBy(c => c.DistanceTo(connectionPoint1.Point)).
            //        Where(c => c.DistanceTo(connectionPoint1.Point) > dTolerance).ToList();

            //    if (mc.Id != connectionPoint1.Element.Id) { conPoints.RemoveAt(0); }
            //    var checkDir = inverse ? dirXYZ.Negate() : dirXYZ;

            //    var aTolerance = 3.DegToRad();
            //    Func<XYZ, bool> func = (c) => Math.Round((connectionPoint1.Point - c).Normalize().AngleTo(checkDir)) == 0;
            //    var foundCon = conPoints.FirstOrDefault(func);
            //    if (foundCon == null)
            //    { aNP = default; }
            //    else
            //    {
            //        aNP = foundCon.ToPoint3d();
            //        aNP = _pointConverter.ConvertToUCS2(aNP).Round(_tolerance);
            //        //_pointVisualisator.Show(aNP);
            //    }

            //    return dir.Round(_tolerance);
            //}


            private (XYZ pointFloorBound, XYZ pointCeilingBound) GetFloorBounds(
                (Element, XYZ) pointElement,
           Document doc,
           double minDistToFloor, double minDistToCeiling,
           bool isInsulationAccount = true,
           int distnaceToFindFloor = 30)
            {
                return pointElement.GetFloorBounds(doc, minDistToFloor, minDistToCeiling,
                    isInsulationAccount, distnaceToFindFloor);
            }

            public ISpecifyConnectionPointBoundaries SetExternalToken1(CancellationTokenSource externalCancellationToken)
            {
                throw new NotImplementedException();
            }

            #endregion

        }
    }
}
