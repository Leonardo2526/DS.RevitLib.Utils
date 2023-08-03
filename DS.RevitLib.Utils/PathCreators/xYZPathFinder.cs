using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Points;
using DS.PathFinder;
using DS.RevitLib.Utils.Bases;
using DS.RevitLib.Utils.Extensions;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.PathCreators
{
    /// <summary>
    /// An object that used to find path between <see cref="Autodesk.Revit.DB.XYZ"/> points.
    /// </summary>
    public class xYZPathFinder : IPathFinder<XYZ>
    {
        private readonly List<Element> _docElements;
        private readonly Dictionary<RevitLinkInstance, List<Element>> _linkElementsDict;
        private readonly PathAlgorithmFactory _algorithmFactory;
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private MEPCurve _startMEPCurve;
        private MEPCurve _endMEPCurve;
        private List<Element> _objectsToExclude = new List<Element>();
        private bool _allowStartDirection;
        private List<PlaneType> _planes;

        /// <summary>
        /// Instantiate an object to find path between <see cref="Autodesk.Revit.DB.XYZ"/> points.
        /// </summary>
        public xYZPathFinder(UIDocument uiDoc, IBasisStrategy basisStrategy, ITraceSettings traceSettings,
            List<Element> docElements, Dictionary<RevitLinkInstance, List<Element>> linkElementsDict = null)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _docElements = docElements;
            _linkElementsDict = linkElementsDict;
            _algorithmFactory = new PathAlgorithmFactory(_uiDoc, basisStrategy, traceSettings, _docElements, _linkElementsDict);
        }

        /// <summary>
        /// Build with some additional paramters.
        /// </summary>
        /// <param name="startMEPCurve"></param>
        /// <param name="endMEPCurve"></param>
        /// <param name="objectsToExclude"></param>
        /// <param name="allowStartDirection"></param>
        /// <param name="planes"></param>
        /// <returns></returns>
        public xYZPathFinder Build(MEPCurve startMEPCurve, MEPCurve endMEPCurve, List<Element> objectsToExclude, 
            bool allowStartDirection = true, List<PlaneType> planes = null) 
        {
            _startMEPCurve = startMEPCurve;
            _endMEPCurve = endMEPCurve;

            //add objectsToExclude with its insulations
            var objectToExcludeIds = objectsToExclude.Select(obj => obj.Id).ToList();
            List<ElementId> insulationIds = ElementUtils.GetInsulation(objectsToExclude);
            foreach (var insId in insulationIds)
            {
                if(!objectToExcludeIds.Contains(insId)) { objectsToExclude.Add(_doc.GetElement(insId)); }
            }

            _objectsToExclude = objectsToExclude;
            _allowStartDirection = allowStartDirection;
            _planes = planes;

            return this;
        }


        /// <inheritdoc/>
        public List<XYZ> Path { get; private set; } = new List<XYZ>();

        /// <inheritdoc/>
        public List<XYZ> FindPath(XYZ startPoint, XYZ endPoint)
        {
            _algorithmFactory.Build(_startMEPCurve, startPoint, endPoint,_objectsToExclude, _planes);
            if (_allowStartDirection) { _algorithmFactory.WithInitialDirections(_startMEPCurve, _endMEPCurve); }

            var maxStepValue = 1000.MMToFeet();
            var dist = startPoint.DistanceTo(endPoint);

            var stepsCount = 20;
            var minStep = 50.MMToFeet();
            var maxStep =  maxStepValue > dist / 3 ? dist / 3 : maxStepValue;
            var stepTemp = stepsCount == 0 ? maxStep : (maxStep - minStep)/ stepsCount;

            IPathFindIterator<Point3d> pathFindIterator = new PathFindIteratorByStep(
                _algorithmFactory,
                _algorithmFactory.StartPoint, _algorithmFactory.EndPoint,
                minStep, maxStep, stepTemp)
            {TokenSource = new CancellationTokenSource(200000)};

            List<Point3d> path3d = pathFindIterator.FindPath();

            if (path3d == null || path3d.Count == 0)
            { TaskDialog.Show("Error", "No available path exist!");}
            else
            { Path = ConvertPath(path3d, _algorithmFactory.PointConverter); }

            return Path;
        }

        /// <inheritdoc/>
        public async Task<List<XYZ>> FindPathAsync(XYZ startPoint, XYZ endPoint)
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
