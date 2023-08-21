using Autodesk.Private.InfoCenterLib;
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
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Various.Bases;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IBasisStrategy _basisStrategy;
        private readonly ITraceSettings _traceSettings;
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
        public xYZPathFinder(IBasisStrategy basisStrategy, ITraceSettings traceSettings)
        {
            _basisStrategy = basisStrategy;
            _traceSettings = traceSettings;
        }

        /// <summary>
        /// Add <see cref="Autodesk.Revit.UI.UIDocument"/> to current object.
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <returns></returns>
        public xYZPathFinder AddDoc(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _basisStrategy.Build(uiDoc);

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
        List<BuiltInCategory> exludedCathegories,
            bool allowStartDirection = true, List<PlaneType> planes = null, MEPCurve basisMEPCurve1 = null, MEPCurve basisMEPCurve2 = null, Basis basis = null,
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
            _allowStartDirection = allowStartDirection;
            _planes = planes;

            if (_basisStrategy is TwoMEPCurvesBasisStrategy twoMCStrategy
                && basisMEPCurve1 is not null
                && basisMEPCurve2 is not null)
            { twoMCStrategy.MEPCurve1 = basisMEPCurve1; twoMCStrategy.MEPCurve2 = basisMEPCurve2; }

            if (basis is not null)
            { _basisStrategy.SetBasis(basis.X, basis.Y, basis.Z); }

            _transactionFactory = transactionFactory;

            return this;
        }

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

        /// <inheritdoc/>
        public List<XYZ> FindPath(ConnectionPoint startPoint, ConnectionPoint endPoint)
        {
            var dir = MEPCurveUtils.GetDirection(_baseMEPCurve);
            var baseMEPCurveBasis = _baseMEPCurve.GetBasisXYZ(dir, startPoint.Point);

            var outline = GetOutline(baseMEPCurveBasis, startPoint.Point, endPoint.Point);
            if (outline == null) { return _path; }

            //show bb
            //var bb = new BoundingBoxXYZ();
            //bb.Min = outline.MinimumPoint;
            //bb.Max = outline.MaximumPoint;
            //var points = bb.GetPoints();
            //points.ForEach(p => { p.Show(_doc); });
            //bb.Show(_doc);
            //return _path;

            (_docElements, _linkElementsDict) = new ElementsExtractor(_doc, _exludedCathegories, outline).GetAll();
            _algorithmFactory = new PathAlgorithmFactory(_uiDoc, _basisStrategy, _traceSettings, baseMEPCurveBasis,
                _docElements, _linkElementsDict, _transactionFactory)
            {
                InsulationAccount = InsulationAccount
            };

            _algorithmFactory.Build(_baseMEPCurve, startPoint, endPoint, outline, _objectsToExclude, _planes);

            if (_algorithmFactory.Algorithm is null) { return _path; }

            if (_allowStartDirection)
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

        //public List<XYZ> FindPath(XYZ startPoint, XYZ endPoint)
        //{
        //    _algorithmFactory.Build(_startMEPCurve, startPoint, endPoint, _outline, _objectsToExclude, _planes);
        //    if (_allowStartDirection) { _algorithmFactory.WithInitialDirections(_startMEPCurve, _endMEPCurve); }

        //    var maxStepValue = 1000.MMToFeet();
        //    var dist = startPoint.DistanceTo(endPoint);

        //    var stepsCount = 10;
        //    var minStep = 50.MMToFeet();
        //    var maxStep = maxStepValue > dist / 3 ? dist / 3 : maxStepValue;
        //    var stepTemp = stepsCount == 0 ? maxStep : (maxStep - minStep) / stepsCount;

        //    IPathFindIterator<Point3d> pathFindIterator = new PathFindIteratorByStep(
        //        _algorithmFactory,
        //        _algorithmFactory.StartPoint, _algorithmFactory.EndPoint,
        //        minStep, maxStep, stepTemp)
        //    { TokenSource = new CancellationTokenSource(200000) };

        //    List<Point3d> path3d = pathFindIterator.FindPath();

        //    if (path3d == null || path3d.Count == 0)
        //    { TaskDialog.Show("Error", "No available path exist!"); }
        //    else
        //    { Path = ConvertPath(path3d, _algorithmFactory.PointConverter); }

        //    return Path;
        //}

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

        private Outline GetOutline(BasisXYZ basisXYZ, XYZ startPoint = null, XYZ endPoint = null)
        {
            XYZ p1;
            if (startPoint == null)
            {
                p1 = _uiDoc.Selection.PickPoint("Укажите первую точку зоны поиска.");
                p1.Show(_doc, 200.MMToFeet());
                _uiDoc.RefreshActiveView();
            }
            else
            { p1 = startPoint; }

            XYZ p2;
            if (startPoint == null)
            {
                p2 = _uiDoc.Selection.PickPoint("Укажите вторую точку зоны поиска.");
                p2.Show(_doc, 200.MMToFeet());
                _uiDoc.RefreshActiveView();
            }
            else
            { p2 = endPoint; }

            var (minPoint, maxPoint) = XYZUtils.CreateMinMaxPoints(new List<XYZ>() { p1, p2 });



            double offsetX = startPoint is null ? 0 : 5000.MMToFeet();
            double offsetY = startPoint is null ? 0 : 5000.MMToFeet();
            double offsetZ = 5000.MMToFeet();

            (XYZ lowerZBound, XYZ topZBound) = GetOffsetZ(startPoint, endPoint, _baseMEPCurve, basisXYZ, offsetZ);
            if (lowerZBound is null || topZBound is null) { return null; }

            var moveVector = new XYZ(XYZ.BasisX.X * offsetX, XYZ.BasisY.Y * offsetY, XYZ.BasisZ.Z * offsetZ);

            var p11 = minPoint + moveVector;
            var p12 = minPoint - moveVector;
            (XYZ minP1, XYZ maxP1) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p11, p12 });

            var p21 = maxPoint + moveVector;
            var p22 = maxPoint - moveVector;
            (XYZ minP2, XYZ maxP2) = XYZUtils.CreateMinMaxPoints(new List<XYZ> { p21, p22 });

            minP1 = new XYZ(minP1.X, minP1.Y, lowerZBound.Z);
            maxP2 = new XYZ(maxP2.X, maxP2.Y, topZBound.Z);

            return new Outline(minP1, maxP2);

            (XYZ bottomZBound, XYZ topZBound) GetOffsetZ(XYZ startPoint, XYZ endPoint, MEPCurve baseMEPCurve, BasisXYZ basis, double offsetZ)
            {
                var bottomZBound = startPoint - XYZ.BasisZ.Multiply(offsetZ);
                var topZBound = startPoint + XYZ.BasisZ.Multiply(offsetZ);

                var startHFloor = startPoint.GetDistanceToFloor(_doc);
                var endHFloor = endPoint.GetDistanceToFloor(_doc);

                var h2 = baseMEPCurve.GetSizeByVector(basis.Z);
                var ins = baseMEPCurve.GetInsulationThickness();
                var hmin = h2 + ins;

                //if no floors
                if (startHFloor == double.PositiveInfinity && endHFloor == double.PositiveInfinity)
                { return (bottomZBound, topZBound); }

                var startZOffset = startHFloor - _traceSettings.H;
                var endZOffset = endHFloor - _traceSettings.H;

                if (startZOffset < hmin)
                {
                    _transactionFactory.CreateAsync(() =>
                TaskDialog.Show("Ошибка", "Расстояние до пола в начальной точке меньше заданного в настройках значения."), "show message");
                    TokenSource?.Cancel(); return (null, null);
                }
                else if (endZOffset < hmin)
                {
                    _transactionFactory.CreateAsync(() =>
                TaskDialog.Show("Ошибка", "Расстояние до пола в конеченой точке меньше заданного в настройках значения."), "show message");
                    TokenSource?.Cancel(); return (null, null);
                }
                else if (startHFloor != double.PositiveInfinity)
                { return (startPoint - XYZ.BasisZ.Multiply(startHFloor - _traceSettings.H), topZBound); }
                else if (endHFloor != double.PositiveInfinity)
                { return (endPoint - XYZ.BasisZ.Multiply(endHFloor - _traceSettings.H), topZBound); }
                else { return (null, null); }
            }
        }
    }
}
