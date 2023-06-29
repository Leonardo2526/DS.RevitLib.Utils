using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Models;
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
    public class CollisionDetectorByTrace : ITraceCollisionDetector
    {
        private readonly Document _doc;
        private readonly MEPCurve _baseMEPCurve;
        private readonly ITraceSettings _traceSettings;
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
        public CollisionDetectorByTrace(Document doc, MEPCurve baseMEPCurve, ITraceSettings traceSettings,
            List<Element> docElements, Dictionary<RevitLinkInstance, List<Element>> linkElementsDict = null)
        {
            _doc = doc;
            _baseMEPCurve = baseMEPCurve;
            _offset = baseMEPCurve.GetInsulationThickness() + traceSettings.D;
            _traceSettings = traceSettings;
            _detectorFactory = new SolidElementCollisionDetectorFactory(doc, docElements, linkElementsDict);
        }

        /// <inheritdoc/>
        public List<ICollision> Collisions { get; private set; } = new List<ICollision>();

        /// <summary>
        /// Check objects 2 to exclude from collisions detection. 
        /// </summary>
        public List<Element> ObjectsToExclude { get; set; }

        ///// <summary>
        ///// Get collisions
        ///// </summary>
        ///// <param name="traceLine"></param>
        ///// <param name="basis"></param>
        ///// <returns>Returns collisions by <see cref="MEPCurve"/> and <paramref name="traceLine"/>.
        ///// <para>
        ///// Returns empty list if no collisions were detected.
        ///// </para>
        ///// </returns>
        //public List<ICollision> GetCollisions(Line traceLine, Basis basis = null)
        //{
        //    XYZ center = traceLine.GetCenter();
        //    XYZ dir = traceLine.Direction;
        //    var p1 = traceLine.GetEndPoint(0);
        //    var p2 = traceLine.GetEndPoint(1);

        //    var checkSolid = _baseMEPCurve.GetOffsetSolid(_traceSettings.D, p1, p2);
        //    return Collisions = _detectorFactory.GetCollisions(checkSolid, ObjectsToExclude);
        //}

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
        public List<ICollision> GetCollisions(Point3D point1, Point3D point2)
        {
            XYZ p1 = point1.ToXYZ();
            XYZ p2 = point2.ToXYZ();

            var checkSolid = _baseMEPCurve.GetOffsetSolid(_offset, p1, p2);
            //new TransactionBuilder(_doc).Build(() => checkSolid.ShowShape(_doc), "Show shape");
            //return new List<ICollision>();
            return Collisions = _detectorFactory.GetCollisions(checkSolid, ObjectsToExclude);
        }
    }
}
