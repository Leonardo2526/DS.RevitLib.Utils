using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Extensions
{
    internal class SolidOffsetExtractor
    {
        private readonly Document _doc;
        private readonly MEPCurve _mEPCurve;
        private readonly double _offset;
        private readonly Line _mEPCurveLine;
        private double _extrusionDist;
        private XYZ _extrusionDir;
        private XYZ _startPoint;
        private XYZ _endPoint;

        public SolidOffsetExtractor(MEPCurve mEPCurve, double offset, XYZ refPoint = null, XYZ extrusionDir = null, double extrusionDist = 0)
        {
            _mEPCurve = mEPCurve;
            _offset = offset;
            _startPoint = refPoint;
            _extrusionDir = extrusionDir;
            _mEPCurveLine = MEPCurveUtils.GetLine(mEPCurve);
            _extrusionDist = extrusionDist;
            //_extrusionDist = extrusionDist == 0 ? MEPCurveUtils.GetLength(mEPCurve) : extrusionDist;
            _doc = mEPCurve.Document;
        }

        public Solid Extract()
        {
            var solid = ElementUtils.GetSolid(_mEPCurve);

            List<Curve> faceCurves = GetFaceCurves(solid);
            _startPoint ??= _mEPCurveLine.Project(faceCurves.FirstOrDefault().GetEndPoint(0)).XYZPoint;
            _extrusionDir ??= GetDirection(_startPoint).Normalize();

            List<Curve> transformFaceCurves = new List<Curve>();
            double dist = _startPoint.DistanceTo(_endPoint);
            _extrusionDist = _extrusionDist == 0 ? dist : _extrusionDist;
            XYZ moveVector = _extrusionDir.Negate() * dist;

            Transform transform = Transform.CreateTranslation(moveVector);
            foreach (var curve in faceCurves)
            {
                transformFaceCurves.Add(curve.CreateTransformed(transform));
            }

            List<Curve> offsetCurves = GetOffsetCurves(solid, transformFaceCurves);

            //connect offseted lines
            List<Line> lines = offsetCurves.OfType<Line>().ToList();
            offsetCurves = offsetCurves.Where(obj => obj is not Line).ToList();

            List<Curve> connectedCurves = lines.Any() ?
                new LinesConnector(lines).Connect().Cast<Curve>().ToList() :
                new List<Curve>();

            offsetCurves.AddRange(connectedCurves);

            return CreateExtrudedSolid(offsetCurves);
        }

        private XYZ GetDirection(XYZ refPoint)
        {
            XYZ p1 = _mEPCurveLine.GetEndPoint(0);
            XYZ p2 = _mEPCurveLine.GetEndPoint(1);
            _endPoint = (refPoint - p1).IsZeroLength() ? p2 : p1;
            return _endPoint - refPoint;
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
    }
}
