using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Extensions;
using PathFinderLib;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.PathCreators
{
    /// <summary>
    /// An object that represents finder to get path.
    /// </summary>
    public class PathFindCreator : IPathCreator
    {
        private Document _doc;
        private double _elbowRadius;
        private XYZ _xVector;
        private double _offset;
        private double _width;
        private double _height;
        private CancellationToken _cancellationToken = CancellationToken.None;

        /// <summary>
        /// Instantiate an object to find path.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elbowRadius"></param>
        /// <param name="xVector">Align vector for path in XY plane.</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="offset">Offset from element</param>
        public PathFindCreator Create(Document doc, double elbowRadius, XYZ xVector,
            double width, double height, double offset = 0)
        {
            _doc = doc;
            _elbowRadius = elbowRadius;
            _xVector = xVector;
            _offset = offset;
            _width = width;
            _height = height;
            return this;
        }

        /// <inheritdoc/>
        public List<ElementId> ExceptionElements { get; set; }

        public CancellationToken CancellationToken { get => _cancellationToken; set => _cancellationToken = value; }

        /// <inheritdoc/>
        public async Task<List<XYZ>> CreateAsync(XYZ point1, XYZ point2)
        {
            var excludedElements = ExceptionElements.Select(obj => obj.IntegerValue).ToList();

            var excludedElementsInsulationIds = new List<ElementId>();
            ExceptionElements.ForEach(obj =>
            {
                if (_doc.GetElement(obj) is Pipe || _doc.GetElement(obj) is Duct)
                {
                    Element insulation = InsulationLiningBase.GetInsulationIds(_doc, obj)?
                  .Select(x => _doc.GetElement(x)).FirstOrDefault();
                    if (insulation != null && insulation.IsValidObject) { excludedElementsInsulationIds.Add(insulation.Id); }
                }
            });
            excludedElements.AddRange(excludedElementsInsulationIds.Select(obj => obj.IntegerValue).ToList());

            var mainOptions = new MainFinderOptions(excludedElements);
            var secondaryOptions = new SecondaryOptions()
            {
                ElbowWidth = _elbowRadius,
                x_y_coef = 1,
                z_coef = 1,
                XVector = _xVector
            };

            //класс анализирует геометрию
            var geometryDocuments = GeometryDocuments.Create(_doc, mainOptions, false);

            //класс для поиска пути
            var finder = new PathFinderToOnePointDefault(point1, point2,
                          _height, _width, _offset, _offset, geometryDocuments, mainOptions, secondaryOptions);
            //ищем путь
            List<XYZ> path = await finder.FindPath(_cancellationToken) ?? new List<XYZ>();
            if (path.Count == 0) return path;

            //объединяем прямые последовательные участки пути в один сегмент
            path = Optimizer.MergeStraightSections(path, mainOptions);

            var zigzag = new ZigZagCleaner(path, mainOptions, secondaryOptions);
            var cleanPath = zigzag.Clear(geometryDocuments, _height, _width, _offset, _offset);

            cleanPath = RefinePath(cleanPath, point1, point2);

            return cleanPath;
        }

        private List<XYZ> RefinePath(List<XYZ> path, XYZ p1, XYZ p2)
        {
            if (path.Count < 3 || ExceptionElements.Count == 0) { return path; }
            if (ExceptionElements.TrueForAll(id => _doc.GetElement(id) is not MEPCurve)) { return path; }

            var pathPostStart = path[1];
            var pathPreEnd = path[path.Count - 2];

            MEPCurve mEPCurve1 = GetMEPCurveInPoint(p1);
            MEPCurve mEPCurvePathPostStart = GetMEPCurveInPoint(pathPostStart);
            if (HasPointToRemove(p1, p2, mEPCurve1, p1, mEPCurvePathPostStart))
            { path.RemoveAt(0); p1 = path[0]; }

            MEPCurve mEPCurve2 = GetMEPCurveInPoint(p2);
            MEPCurve mEPCurvePathPreEnd = GetMEPCurveInPoint(pathPreEnd);
            if (HasPointToRemove(p1, p2, mEPCurve2, p2, mEPCurvePathPreEnd))
            { path.RemoveAt(path.Count - 1); p2 = path[path.Count - 1]; }

            return path;
        }

        private MEPCurve GetMEPCurveInPoint(XYZ point)
        {
            MEPCurve mEPCurve = null;

            foreach (var id in ExceptionElements)
            {
                var elem = _doc.GetElement(id) as MEPCurve;
                if (elem is not null)
                {
                    var solid = ElementUtils.GetSolid(elem);
                    if (solid.Contains(point)) { mEPCurve = elem; break; }
                }
            }

            return mEPCurve;
        }

        private bool HasPointToRemove(XYZ startPoint, XYZ endPoint, MEPCurve mEPCurveInEdgePoint, XYZ point, MEPCurve mEPCurveInPoint)
        {
            return mEPCurveInEdgePoint != null
              && mEPCurveInPoint != null
              && mEPCurveInEdgePoint.Id == mEPCurveInPoint.Id
              && !point.IsBetweenPoints(startPoint, endPoint);
        }
    }
}
