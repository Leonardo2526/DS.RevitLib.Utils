using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Basis;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Points;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids;
using DS.RevitLib.Utils.Various.Bases;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace DS.RevitLib.Utils.Collisions.Detectors
{
    /// <summary>
    /// An object that detect collisions (intersections) between objects in Revit model by trace and <see cref="MEPCurve"/>.
    /// </summary>
    public class CollisionDetectorByTrace : ITraceCollisionDetector<Point3d>
    {
        private readonly Document _doc;
        private readonly MEPCurve _baseMEPCurve;
        private readonly ITraceSettings _traceSettings;
        private readonly IPoint3dConverter _pointConverter;
        private readonly ITransactionFactory _transactionFactory;
        private readonly SolidElementCollisionDetectorFactory _detectorFactory;
        private readonly double _offset;
        private BasisXYZ _sourceBasis;
        private Point3d _startPoint;
        private Point3d _endPoint;
        private ConnectionPoint _startConnectionPoint;
        private ConnectionPoint _endConnectionPoint;

        /// <summary>
        /// Instantiate an object to create objects for collisions (intersections) detection 
        /// between objects in Revit model by trace and <paramref name="baseMEPCurve"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="baseMEPCurve"></param>
        /// <param name="traceSettings"></param>
        /// <param name="docElements"></param>
        /// <param name="linkElementsDict"></param>
        /// <param name="pointConverter"></param>
        public CollisionDetectorByTrace(Document doc, MEPCurve baseMEPCurve, ITraceSettings traceSettings,
            List<Element> docElements, Dictionary<RevitLinkInstance, List<Element>> linkElementsDict = null, IPoint3dConverter pointConverter = null,
            ITransactionFactory transactionFactory = null)
        {
            _doc = doc;
            _baseMEPCurve = baseMEPCurve;
            _offset = baseMEPCurve.GetInsulationThickness() + traceSettings.B - 0.03;
            _traceSettings = traceSettings;
            _pointConverter = pointConverter;
            _transactionFactory = transactionFactory;
            _transactionFactory ??= new ContextTransactionFactory(_doc);
            _detectorFactory = new SolidElementCollisionDetectorFactory(doc, docElements, linkElementsDict);
            _detectorFactory.MinVolume = 0;
            SolidExtractor = new BestSolidOffsetExtractor(baseMEPCurve, _offset);
        }

        /// <inheritdoc/>
        public List<ICollision> Collisions { get; private set; } = new List<ICollision>();

        /// <summary>
        /// Check objects 2 to exclude from collisions detection. 
        /// </summary>
        public List<Element> ObjectsToExclude { get; set; }

        /// <summary>
        /// Specify if check offset <see cref="Solid"/> will be created up to the end point + <see cref="Solid"/> offset.
        /// </summary>
        public bool OffsetOnEndPoint { get; set; } = false;

        public BestSolidOffsetExtractor SolidExtractor { get; }

        public ConnectionPoint StartConnectionPoint
        {
            get => _startConnectionPoint;
            set 
            { 
                _startConnectionPoint = value;
                _startPoint = _pointConverter.ConvertToUCS2(value.Point.ToPoint3d());
            }
        }

        public ConnectionPoint EndConnectionPoint
        {
            get => _endConnectionPoint;
            set
            {
                _endConnectionPoint = value;
                _endPoint = _pointConverter.ConvertToUCS2(value.Point.ToPoint3d());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="basis"></param>
        /// <returns>
        /// Returns collisions by <see cref="MEPCurve"/> between <paramref name="point1"/> and <paramref name="point2"/>.
        /// <para>
        /// Returns empty list if no collisions were detected.
        /// </para>
        /// </returns>
        public List<ICollision> GetCollisions(Point3d point1, Point3d point2, Basis3d basis)
        {
            XYZ p1 = null;
            XYZ p2 = null;

            var endSolidPoint = point2;
            if (OffsetOnEndPoint)
            {
                var v = point2 - point1;
                v = Vector3d.Divide(v, v.Length);
                var size = _baseMEPCurve.GetMaxSize();
                var mult = size + _offset;
                endSolidPoint += Vector3d.Multiply(v, mult);
            }

            if (_pointConverter is not null)
            {
                Point3d point1UCS1 = _pointConverter.ConvertToUCS1(point1);
                Point3d point2UCS1 = _pointConverter.ConvertToUCS1(endSolidPoint);
                p1 = new XYZ(point1UCS1.X, point1UCS1.Y, point1UCS1.Z);
                p2 = new XYZ(point2UCS1.X, point2UCS1.Y, point2UCS1.Z);
            }
            else
            {
                p1 = new XYZ(point1.X, point1.Y, point1.Z);
                p2 = new XYZ(endSolidPoint.X, endSolidPoint.Y, endSolidPoint.Z);
            }


            var uCS1Basis = _pointConverter.ConvertToUCS1(basis).ToXYZ();
            var checkSolid = SolidExtractor.Extract(p1, p2, uCS1Basis);
            return Collisions = _detectorFactory.GetCollisions(checkSolid, ObjectsToExclude);

            _transactionFactory.CreateAsync(() =>
            {
                checkSolid.ShowShape(_doc);
            }
            , "Show shape");
            //return new List<ICollision>();
            //return Collisions = _detectorFactory.GetCollisions(checkSolid, ObjectsToExclude);
        }

        public List<ICollision> GetFirstCollisions(Point3d point2, Basis3d basis)
        {
            var point1 = _startPoint;

            if (_startConnectionPoint is null) { return GetCollisions(point1, point2, basis); }

            var connectedElements = ConnectorUtils.GetConnectedElements(_startConnectionPoint.Element);

            var cacheExcluded = new List<Element>();
            cacheExcluded.AddRange(ObjectsToExclude);

            ObjectsToExclude = ObjectsToExclude.Union(connectedElements).ToList();

            var collisions = GetCollisions(point1, point2, basis);
            ObjectsToExclude = cacheExcluded;

            return collisions;
        }

        public List<ICollision> GetLastCollisions(Point3d point1, Basis3d basis)
        {
            var point2 = _endPoint;

            if (_endConnectionPoint is null) { return GetCollisions(point1, point2, basis); }

            var connectedElements = ConnectorUtils.GetConnectedElements(_endConnectionPoint.Element);

            var cacheExcluded = new List<Element>();
            cacheExcluded.AddRange(ObjectsToExclude);

            ObjectsToExclude = ObjectsToExclude.Union(connectedElements).ToList();

            var collisions = GetCollisions(point1, point2, basis);
            ObjectsToExclude = cacheExcluded;

            return collisions;
        }
    }
}
