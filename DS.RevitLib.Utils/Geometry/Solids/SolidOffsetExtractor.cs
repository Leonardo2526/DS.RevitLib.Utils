using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Extensions
{
    internal class SolidOffsetExtractor
    {
        private readonly Document _doc;
        private readonly MEPCurve _mEPCurve;
        private readonly double _offset;
        private readonly Line _mEPCurveLine;
        private readonly XYZ _startPoint;
        private readonly XYZ _endPoint;
        private double _extrusionDist;
        private XYZ _extrusionDir;

        /// <summary>
        /// Create a new instance of object to extract solid from <paramref name="mEPCurve"/>.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="offset"></param>
        public SolidOffsetExtractor(MEPCurve mEPCurve, double offset)
        {
            _mEPCurve = mEPCurve;
            _offset = offset;
            _mEPCurveLine = MEPCurveUtils.GetLine(mEPCurve);
            _doc = mEPCurve.Document;

            _startPoint = _mEPCurveLine.GetEndPoint(0);
            _endPoint = _mEPCurveLine.GetEndPoint(1);
        }

        /// <summary>
        /// Create a new instance of object to extract solid from <paramref name="mEPCurve"/>
        /// between <paramref name="startPoint"/> and <paramref name="endPoint"/>.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="offset"></param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        public SolidOffsetExtractor(MEPCurve mEPCurve, double offset, XYZ startPoint, XYZ endPoint)
        {
            _mEPCurve = mEPCurve;
            _offset = offset;
            _mEPCurveLine = MEPCurveUtils.GetLine(mEPCurve);
            _doc = mEPCurve.Document;

            _startPoint = startPoint;
            _endPoint = endPoint;
        }

        /// <summary>
        /// Extract <see cref="Solid"/> from <see cref="_mEPCurve"/>.
        /// </summary>
        /// <returns></returns>
        public Solid Extract()
        {
            _extrusionDir = (_endPoint - _startPoint).Normalize();
            _extrusionDist = _startPoint.DistanceTo(_endPoint);

            var solid = ElementUtils.GetSolid(_mEPCurve);
            List<Curve> faceCurves = GetFaceCurves(solid);

            //move face curves to start point
            var faceCenterPoint = _mEPCurveLine.Project(faceCurves.FirstOrDefault().GetEndPoint(0)).XYZPoint;
            XYZ moveVector = _startPoint - faceCenterPoint;
            faceCurves = moveVector.IsZeroLength() ? faceCurves : GetTransformed(moveVector, faceCurves);

            List<Curve> offsetCurves = GetOffsetCurves(solid, faceCurves);

            //connect offseted lines
            List<Line> lines = offsetCurves.OfType<Line>().ToList();
            offsetCurves = offsetCurves.Where(obj => obj is not Line).ToList();

            List<Curve> connectedCurves = lines.Any() ?
                new LinesConnector(lines).Connect().Cast<Curve>().ToList() :
                new List<Curve>();

            offsetCurves.AddRange(connectedCurves);

            return CreateExtrudedSolid(offsetCurves);
        }


        #region PrivateMethods

        private List<Curve> GetTransformed(XYZ moveVector, List<Curve> curves)
        {
            var transformFaceCurves = new List<Curve>();
            Transform transform = Transform.CreateTranslation(moveVector);
            foreach (var curve in curves)
            {
                transformFaceCurves.Add(curve.CreateTransformed(transform));
            }
            return transformFaceCurves;
        }

        private List<Curve> GetFaceCurves(Solid solid)
        {
            var faceCurves = new List<Curve>();

            Face face1 = solid.Faces.get_Item(0);
            EdgeArray edgeArray1 = face1.EdgeLoops.get_Item(0);
            for (int i = 0; i < 4; i++)
            {
                Edge edge = edgeArray1.get_Item(i);
                var curve = edge.AsCurve();
                faceCurves.Add(curve);
            }

            return faceCurves;
        }

        private List<Curve> GetOffsetCurves(Solid solid, List<Curve> curves)
        {
            var offsectCurves = new List<Curve>();
            Face face1 = solid.Faces.get_Item(0);
            EdgeArray edgeArray1 = face1.EdgeLoops.get_Item(0);

            Face face2 = solid.Faces.get_Item(1);
            EdgeArray edgeArray2 = face2.EdgeLoops.get_Item(0);

            var edgeArrayPoints1 = edgeArray1.GetPolygon();
            var edgeArrayPoints2 = edgeArray2.GetPolygon();
            XYZ edgeAttayCenter1 = XYZUtils.GetAverage(edgeArrayPoints1);
            XYZ edgeAttayCenter2 = XYZUtils.GetAverage(edgeArrayPoints2);
            XYZ dir = edgeAttayCenter1 - edgeAttayCenter2;

            foreach (var curve in curves)
            {
                var offsetCurve = curve.CreateOffset(_offset, dir);
                offsectCurves.Add(offsetCurve);
            }

            return offsectCurves;
        }

        private Solid CreateExtrudedSolid(List<Curve> curves)
        {
            var cloop = new CurveLoop();

            foreach (var curve in curves)
            {
                cloop.Append(curve);
            }


            IList<CurveLoop> loop = new List<CurveLoop>() { cloop };

            return GeometryCreationUtilities.CreateExtrusionGeometry(loop, _extrusionDir, _extrusionDist);
        }

        #endregion

    }
}
