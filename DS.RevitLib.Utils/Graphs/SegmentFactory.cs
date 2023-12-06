using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP;
using QuickGraph;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Line = Rhino.Geometry.Line;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// An object that represents factory to get free segments.
    /// </summary>
    public class SegmentFactory
    {
        private readonly Document _doc;
        private readonly IElementCollisionDetector _collisionDetector;
        private IEnumerable<Line> _segments = new List<Line>();

        /// <summary>
        /// Instansiate an object that represents factory to get free segments.
        /// <para>
        /// Get segments that hasn't collisions and are content with specified parameters.
        /// </para>
        /// </summary>
        public SegmentFactory(Document doc,
            IElementCollisionDetector collisionDetector)
        {
            _doc = doc;
            _collisionDetector = collisionDetector;
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Line> Segments
        { get => _segments; private set => _segments = value; }

        /// <summary>
        /// Minimum distance from <see cref="Segments"/> end points to edge <see cref="Autodesk.Revit.DB.Connector"/>s.
        /// <para>
        /// If <see cref="MinDistanceToElements"/> is greater than this value it will be replaced.
        /// </para>
        /// </summary>
        public double MinDistanceToConnector { get; set; }

        /// <summary>
        /// Minimum distance from <see cref="Segments"/> end points to <see cref="Document"/> <see cref="Autodesk.Revit.DB.Element"/>s.
        /// </summary>
        public double MinDistanceToElements { get; set; }

        /// <summary>
        /// Minimum distance from source edge vertex.
        /// </summary>
        public double MinDistanceFromSource { get; set; }

        /// <summary>
        /// Specifies whether consider insulation thickness or not.
        /// </summary>
        public bool IsInsulationAccount { get; set; }


        /// <summary>
        /// Get <see cref="Line"/>s from <paramref name="edge"/> that are content with specified parameters.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public IEnumerable<Line> GetFreeSegments(IEdge<IVertex> edge)
        {
            var mEPCurve = edge.TryGetMEPCurve(_doc);
            if (mEPCurve == null) { return null; }

            var edgeLine = edge.GetLocationLine(_doc);
            var sourceLoc = edge.Source.GetLocation(_doc).ToPoint3d();

            var minDistToConnector = Math.Max(MinDistanceToConnector, MinDistanceToElements);
            var sourceOdd = TryGetOddsToDist(edge.Source, mEPCurve, _doc);
            var targetOdd = TryGetOddsToDist(edge.Target, mEPCurve, _doc);

            var connectionSegment = edge.GetConnectionSegment(_doc, minDistToConnector + sourceOdd, minDistToConnector + targetOdd);
            var deductionSegments = GetIntersectionSegments(mEPCurve, edgeLine, _collisionDetector).ToList();

            //add as option minSourceLine
            if (deductionSegments.Any())
            {
                deductionSegments = deductionSegments.OrderBy(s => s.PointAtLength(s.Length / 2).DistanceTo(sourceLoc)).ToList();
                double minLongLeg = MinDistanceFromSource + MinDistanceToConnector;
                Line minSourceLine1 = GetMinLineSource(edge, deductionSegments.FirstOrDefault().To, minLongLeg, _doc);
                if (minSourceLine1.Length > 0)
                { deductionSegments.Add(minSourceLine1); }
                Line minSourceLine2 = GetMinLineSource(edge, deductionSegments.FirstOrDefault().From, minLongLeg, _doc);
                if (minSourceLine2.Length > 0)
                { deductionSegments.Add(minSourceLine2); }
            }

            var freeSegments = LineBooleanTools.Substract(connectionSegment, deductionSegments);

            Segments = freeSegments.OrderBy(s => s.PointAtLength(s.Length / 2).DistanceTo(sourceLoc)).ToList();
            return Segments;
        }


        private IEnumerable<Line> GetIntersectionSegments(MEPCurve mEPCurve, Line line, IElementCollisionDetector collisionDetector)
        {
            if (!line.Extend(100, 100)) { return null; }
            var xYZLine = line.ToXYZ();

            var collisionSegements = new List<Line>();

            var collisions = collisionDetector.GetCollisions(mEPCurve);

            foreach (var collision in collisions)
            {
                var intersectionSolid = collision.GetIntersectionSolid();
                (var sp1, var sp2) = intersectionSolid is null ?
                    (null, null) :
                    intersectionSolid.GetEdgeProjectPoints(xYZLine);
                if (sp1 != null && sp2 != null)
                {
                    var segment = new Line(sp1.ToPoint3d(), sp2.ToPoint3d());
                    segment.Extend(MinDistanceToElements, MinDistanceToElements);
                    collisionSegements.Add(segment);
                }
            }

            return collisionSegements;
        }

        /// <summary>
        /// Consider odds like spuds.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="mEPCurve"></param>
        /// <param name="doc"></param>
        /// <returns>
        /// Additional distance from odd.
        /// <para>
        /// 0 if no odds.
        /// </para>
        /// </returns>
        private double TryGetOddsToDist(IVertex vertex, MEPCurve mEPCurve, Document doc)
        {
            double odds = 0;

            var famInst = vertex.TryGetFamilyInstance(doc);

            if (famInst is not null && famInst.IsSpud())
            {
                var (parents, child) = famInst.GetConnectedElements();
                if (child is not null && child is MEPCurve childMEPCurve && child.Id != mEPCurve.Id)
                {
                    (double width, double heigth) = childMEPCurve.GetOuterWidthHeight();
                    var mEPCurveSize = Math.Max(width, heigth);
                    var insulationThickness = IsInsulationAccount ?
                        childMEPCurve.GetInsulationThickness()
                        : 0;
                    odds = mEPCurveSize / 2 + insulationThickness;
                }
            }

            return odds;
        }

        private Line GetMinLineSource(IEdge<IVertex> edge, Point3d sourceLocation, double minLongLeg, Document doc)
        {
            Line line = default;

            if (minLongLeg > 0)
            {
                var dir = edge.Direction(doc);
                line = new Line(sourceLocation, sourceLocation + Vector3d.Multiply(dir, minLongLeg));
            }

            return line;
        }
    }
}
