using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Points;
using DS.GraphUtils.Entities;
using DS.PathFinder;
using DS.PathFinder.Algorithms.AStar;
using DS.RevitLib.Utils.Bases;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.Models;
using QuickGraph;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DS.RevitLib.Utils.PathCreators.AlgorithmBuilder
{
    /// <summary>
    /// An object to build a new path find algorythm..
    /// </summary>
    public partial class PathAlgorithmBuilder : IAlgorithmBuilder, IToleranceUpdater
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private IBasisStrategy _basisStrategy;
        private readonly ITraceSettings _traceSettings;
        private readonly IEnumerable<IBasisStrategy> _basisStrategies;
        private AStarAlgorithmCDF _algorithm = new();
        private IDirectionValidator _directionValidator;
        private MEPCurve _baseMEPCurve;
        private ITransactionFactory _transactionFactory;

        /// <summary>
        /// Instanciate an object to build a new path find algorythm.
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <param name="traceSettings"></param>
        /// <param name="basisStrategies"></param>
        /// <param name="quadrantModel"></param>
        public PathAlgorithmBuilder(
            UIDocument uiDoc,
            ITraceSettings traceSettings,
            IEnumerable<IBasisStrategy> basisStrategies,
            IDirectionValidator quadrantModel)
        {
            _uiDoc = uiDoc;
            _traceSettings = traceSettings;
            _basisStrategies = basisStrategies;
            _doc = uiDoc.Document;

            //quadrantModel.DisableQuadrants();
            //var dir1 = new Vector3d(0, 0, 1);
            //quadrantModel.EnableQuadrants(OrthoPlane.XY);
            //quadrantModel.EnableQuadrants(dir1, OrthoPlane.XZ);
            //var dir2 = new Vector3d(0, 0, -1);
            //quadrantModel.EnableQuadrants(dir2, OrthoPlane.XZ);
            _directionValidator = quadrantModel;
        }


        #region Properties

        /// <summary>
        /// Start path find point.
        /// </summary>
        public Point3d StartPoint { get; private set; }

        /// <summary>
        /// End path find point.
        /// </summary>
        public Point3d EndPoint { get; private set; }

        /// <summary>
        /// Builder for <see cref="PathNode"/>.
        /// </summary>
        public NodeBuilder NodeBuilder { get; private set; }

        /// <summary>
        /// Converter to transform geometry objects between UCS.
        /// </summary>
        public IPoint3dConverter PointConverter { get; private set; }

        /// <inheritdoc/>
        public IPathFindAlgorithm<Point3d, Point3d> Algorithm { get => _algorithm; }

        /// <summary>
        /// 
        /// </summary>
        public ITransactionFactory TransactionFactory
        {
            get
            {
                return _transactionFactory ??=
                    new ContextTransactionFactory(_doc, RevitContextOption.Auto);
            }
            set => _transactionFactory = value;
        }

        /// <summary>
        /// Strategy to get next connection point on source and target.
        /// </summary>
        public INextConnectionPointStrategy NextPointStrategy { get; set; }


        #endregion

        /// <summary>
        /// Set base <see cref="MEPCurve"/> and path find basis.
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <param name="basisMEPCurve1"></param>
        /// <param name="basisMEPCurve2"></param>
        /// <param name="allowSecondElementForBasis"></param>
        /// <returns>
        /// Object to build next.
        /// </returns>
        public ISpecifyExclusions SetBasis(MEPCurve baseMEPCurve,
                MEPCurve basisMEPCurve1, MEPCurve basisMEPCurve2, bool allowSecondElementForBasis)
        {
            _baseMEPCurve = baseMEPCurve;
            BuildBasisStrategy(basisMEPCurve1, basisMEPCurve2, allowSecondElementForBasis, null);

            return new Impl(this);
        }

        /// <inheritdoc/>
        public void Update(int tolerance)
        {
            _algorithm.MToleranceCoef = tolerance;
        }

        private void BuildBasisStrategy(MEPCurve basisMEPCurve1, MEPCurve basisMEPCurve2, bool allowSecondElementForBasis, Basis basis)
        {
            if (allowSecondElementForBasis)
            {
                _basisStrategy = _basisStrategies.FirstOrDefault(s => s is TwoMEPCurvesBasisStrategy);
                var twoMCStrategy = _basisStrategy as TwoMEPCurvesBasisStrategy;
                twoMCStrategy.MEPCurve1 = basisMEPCurve1; twoMCStrategy.MEPCurve2 = basisMEPCurve2;
            }
            else
            {
                _basisStrategy = _basisStrategies.FirstOrDefault(s => s is OneMEPCurvesBasisStrategy);
                var oneMCStrategy = _basisStrategy as OneMEPCurvesBasisStrategy;
                oneMCStrategy.MEPCurve1 = basisMEPCurve1;
            }
            _basisStrategy.Build(_uiDoc);
            _basisStrategy.GetBasis();

            if (basis is not null)
            { _basisStrategy.SetBasis(basis.X, basis.Y, basis.Z); }
        }

    }

    #region Interfaces

    /// <summary>
    /// The interface to specify boundaries.
    /// </summary>
    public interface ISpecifyConnectionPointBoundaries
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="outlineFactory"></param>
        /// <param name="externalOutline"></param>
        /// <param name="accountInitialDirections"></param>
        /// <returns></returns>
        ISpecifyParameter SetBoundaryConditions(
            ConnectionPoint startPoint,
            ConnectionPoint endPoint,
            IOutlineFactory outlineFactory = null,
            Outline externalOutline = null,
            bool accountInitialDirections = false);
    }

    #endregion

}
