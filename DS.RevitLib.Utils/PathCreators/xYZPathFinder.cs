using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Points;
using DS.PathFinder;
using DS.PathFinder.Algorithms.Enumeratos;
using DS.RevitLib.Utils.Bases;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Various.Bases;
using Rhino.Geometry;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.PathCreators
{
    /// <summary>
    /// An object that used to find path between <see cref="Autodesk.Revit.DB.XYZ"/> points.
    /// </summary>
    public class xYZPathFinder : IPathFinder<ConnectionPoint, XYZ>
    {
        private List<Element> _docElements;
        private Dictionary<RevitLinkInstance, List<Element>> _linkElementsDict;
        private PathAlgorithmFactory _algorithmFactory;
        private UIDocument _uiDoc;
        private IBasisStrategy _basisStrategy;
        private readonly IEnumerable<IBasisStrategy> _basisStrategies;
        private readonly ITraceSettings _traceSettings;
        private IOutlineFactory _outlineFactory;
        private List<BuiltInCategory> _exludedCathegories;
        private Document _doc;
        private List<Element> _objectsToExclude = new List<Element>();
        private bool _allowStartDirection;
        private List<PlaneType> _planes;
        private List<XYZ> _path = new List<XYZ>();
        private MEPCurve _baseMEPCurve;
        private ITransactionFactory _transactionFactory;

        /// <summary>
        /// Instantiate an object to find path between <see cref="Autodesk.Revit.DB.XYZ"/> points.
        /// </summary>
        public xYZPathFinder(IEnumerable<IBasisStrategy> basisStrategies, ITraceSettings traceSettings)
        {
            _basisStrategies = basisStrategies;
            _traceSettings = traceSettings;
        }

        #region Properties

        /// <inheritdoc/>
        public List<XYZ> Path { get => _path; }

        /// <summary>
        /// Token to cancel finding path operation.
        /// </summary>
        public CancellationTokenSource TokenSource { get; set; }

        /// <summary>
        /// Specifies whether allow insulation collisions on resolving or not.
        /// </summary>
        public bool InsulationAccount { get; set; }

        /// <summary>
        /// Factory to create bounds to find path.
        /// </summary>
        public IOutlineFactory OutlineFactory { get => _outlineFactory; set => _outlineFactory = value; }

        /// <summary>
        /// External bound <see cref="Autodesk.Revit.DB.Outline"/>.
        /// <para>
        /// If specified pathFinder will not be able to create bound less or more than this.
        /// </para>
        /// </summary>
        public Outline ExternalOutline { get; set; }

        /// <summary>
        /// Specifies if account initial connection elements directios to perform correct connection.
        /// </summary>
        public bool AccountInitialDirections { get; set; }


        #endregion



        /// <summary>
        /// Add <see cref="Autodesk.Revit.UI.UIDocument"/> to current object.
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <returns></returns>
        public xYZPathFinder AddDoc(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;

            return this;
        }

        /// <summary>
        /// Build with some additional paramters.
        /// </summary>
        /// <param name="baseMEPCurve"></param>
        /// <param name="objectsToExclude"></param>
        /// <param name="exludedCathegories"></param>
        /// <param name="allowStartDirection"></param>
        /// <param name="planes"></param>
        /// <param name="basisMEPCurve1"></param>
        /// <param name="basisMEPCurve2"></param>
        /// <param name="basis"></param>
        /// <param name="transactionFactory"></param>
        /// <returns></returns>
        public xYZPathFinder Build(MEPCurve baseMEPCurve, List<Element> objectsToExclude,
        List<BuiltInCategory> exludedCathegories, bool allowSecondElementForBasis = false,
        List<PlaneType> planes = null, MEPCurve basisMEPCurve1 = null, MEPCurve basisMEPCurve2 = null, Basis basis = null,
            ITransactionFactory transactionFactory = null)
        {
            _baseMEPCurve = baseMEPCurve;

            //add objectsToExclude with its insulations
            var objectToExcludeIds = objectsToExclude.Select(obj => obj.Id).ToList();

            var insulations = new List<Element>();
            foreach (var item in objectsToExclude)
            {
                InsulationLiningBase ins = null;
                try
                {
                    ins = InsulationLiningBase.GetInsulationIds(item.Document, item.Id).
                        Select(x => item.Document.GetElement(x) as InsulationLiningBase).FirstOrDefault();
                }
                catch (Exception)
                { }
                if (ins is not null && !objectToExcludeIds.Contains(ins.Id)) { insulations.Add(_doc.GetElement(ins.Id)); }
            }

            objectsToExclude.AddRange(insulations);
            _objectsToExclude = objectsToExclude;
            _exludedCathegories = exludedCathegories;
            _planes = planes;

            BuildBasisStrategy(basisMEPCurve1, basisMEPCurve2, allowSecondElementForBasis, basis);

            _transactionFactory = transactionFactory;

            return this;
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

        /// <inheritdoc/>
        public List<XYZ> FindPath(ConnectionPoint startPoint, ConnectionPoint endPoint)
        {
            var dir = MEPCurveUtils.GetDirection(_baseMEPCurve);
            var baseMEPCurveBasis = _baseMEPCurve.GetBasisXYZ(dir, startPoint.Point);

            var outline = BuildOutline(_traceSettings, _baseMEPCurve, startPoint.Point, endPoint.Point);
            if (outline == null) { return _path; }

            //show bb
            //outline.Show(_doc, _transactionFactory);
            //return _path;

            (_docElements, _linkElementsDict) = new ElementsExtractor(_doc, _exludedCathegories, outline).GetAll();
            _algorithmFactory = new PathAlgorithmFactory(_uiDoc, _basisStrategy, _traceSettings, baseMEPCurveBasis,
                _docElements, _linkElementsDict, _transactionFactory)
            {
                InsulationAccount = InsulationAccount
            };

            _algorithmFactory.Build(_baseMEPCurve, startPoint, endPoint, outline, _objectsToExclude, _planes);

            if (_algorithmFactory.Algorithm is null) { return _path; }

            if (AccountInitialDirections)
            {
                var startMEPCurve = startPoint.GetMEPCurve(_objectsToExclude.Select(o => o.Id));
                var endMEPCurve = endPoint.GetMEPCurve(_objectsToExclude.Select(o => o.Id));
                if (startMEPCurve == null || endMEPCurve == null)
                { throw new ArgumentNullException("Failed to find MEPCurve on connection point."); }
                _algorithmFactory.WithInitialDirections();
            }
            _algorithmFactory.Algorithm.ExternalTokenSource = TokenSource;

            var dist = double.MaxValue;
            //var dist = startPoint.Point.DistanceTo(endPoint.Point) / 3;
            var stepEnumerator = new StepEnumerator(_algorithmFactory.NodeBuilder, dist.FeetToMM(), _traceSettings.Step.FeetToMM(), true);
            var heuristicEnumerator = new HeuristicEnumerator(_algorithmFactory.NodeBuilder, true);
            var toleranceEnumerator = new ToleranceEnumerator(_algorithmFactory, true, _traceSettings.A != 90 && _traceSettings.A != 45);
            var pathFindEnumerator = new PathFindEnumerator(stepEnumerator, heuristicEnumerator,
                toleranceEnumerator, _algorithmFactory)
            {
                TokenSource = TokenSource
            };

            var path = new List<Point3d>();
            while (pathFindEnumerator.MoveNext())
            { path = pathFindEnumerator.Current; }

            if (path == null || path.Count == 0)
            {
                TaskDialog.Show("Error", "No available path exist!");
            }
            else
            { _path = ConvertPath(path, _algorithmFactory.PointConverter); }

            return _path;
        }

        private Outline BuildOutline(ITraceSettings traceSettings, MEPCurve mEPCurve, XYZ point1, XYZ point2)
        {
            _outlineFactory ??= new OutlineFactory(_doc);
            if (_outlineFactory is not OutlineFactory outlineFactory) { return null; }

            double defaultOffset = 5000.MMToFeet();
            outlineFactory.XOffset = defaultOffset;
            outlineFactory.YOffset = defaultOffset;
            outlineFactory.ZOffset = defaultOffset;

            var h2 = mEPCurve.GetSizeByVector(XYZ.BasisZ);
            var ins = mEPCurve.GetInsulationThickness();
            var hmin = h2 + ins;
            double offsetFromFloor = hmin + traceSettings.H;
            double offsetFromCeiling = hmin + traceSettings.B;

            outlineFactory.MinHFloor = offsetFromFloor;
            outlineFactory.MinHCeiling = offsetFromCeiling;

            var outline = outlineFactory.Create(point1, point2);
            if (String.IsNullOrEmpty(outlineFactory.ErrorMessage))
            {
                if (ExternalOutline is null) { return outline; }
                else { return outline.GetIntersection(ExternalOutline); }
            }
            else
            {
                _transactionFactory.CreateAsync(() =>
               TaskDialog.Show("Ошибка", outlineFactory.ErrorMessage), "show message"); ;
                TokenSource?.Cancel(); return null;
            }

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
