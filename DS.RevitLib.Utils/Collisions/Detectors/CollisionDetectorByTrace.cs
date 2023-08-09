using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Points;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Models;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        private readonly SolidElementCollisionDetectorFactory _detectorFactory;
        private readonly double _offset;

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
            List<Element> docElements, Dictionary<RevitLinkInstance, List<Element>> linkElementsDict = null, IPoint3dConverter pointConverter = null)
        {
            _doc = doc;
            _baseMEPCurve = baseMEPCurve;
            _offset = baseMEPCurve.GetInsulationThickness() + traceSettings.B;
            _traceSettings = traceSettings;
            _pointConverter = pointConverter;
            _detectorFactory = new SolidElementCollisionDetectorFactory(doc, docElements, linkElementsDict);
            _detectorFactory.MinVolume = 0;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns>
        /// Returns collisions by <see cref="MEPCurve"/> between <paramref name="point1"/> and <paramref name="point2"/>.
        /// <para>
        /// Returns empty list if no collisions were detected.
        /// </para>
        /// </returns>
        public List<ICollision> GetCollisions(Point3d point1, Point3d point2)
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

            var checkSolid = _baseMEPCurve.GetOffsetSolid(_offset, p1, p2);
            //new TransactionBuilder(_doc).Build(() =>
            //{
            //    checkSolid.ShowShape(_doc);
            //}
            //, "Show shape");
            //return new List<ICollision>();
            return Collisions = _detectorFactory.GetCollisions(checkSolid, ObjectsToExclude);
        }
    }
}
