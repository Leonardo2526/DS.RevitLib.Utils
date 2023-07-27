using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Points;
using DS.PathFinder;
using DS.RevitLib.Utils.Bases;
using Rhino.Geometry;
using System.Collections.Generic;
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
        private MEPCurve _baseMEPCurve;
        private double _step;
        private List<Element> _objectsToExclude = new List<Element>();

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
        /// <param name="baseMEPCurve"></param>
        /// <param name="step"></param>
        /// <param name="objectsToExclude"></param>
        /// <returns></returns>
        public xYZPathFinder Build(MEPCurve baseMEPCurve, double step, List<Element> objectsToExclude)
        {
            _baseMEPCurve = baseMEPCurve;
            _step = step;
            _objectsToExclude = objectsToExclude;

            return this;
        }


        /// <inheritdoc/>
        public List<XYZ> Path { get; private set; } = new List<XYZ>();

        /// <inheritdoc/>
        public List<XYZ> FindPath(XYZ startPoint, XYZ endPoint)
        {
            _algorithmFactory.Build(_baseMEPCurve, startPoint, endPoint, _step, _objectsToExclude);
            var algorithm = _algorithmFactory.Create();
            List<Point3d> path3d = algorithm?.FindPath(_algorithmFactory.StartPoint, _algorithmFactory.EndPoint);

            if (path3d == null || path3d.Count == 0)
            { TaskDialog.Show("Error", "No available path exist!"); }
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
