using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Transforms;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace DS.RevitLib.Utils.Extensions
{
    internal class SolidOffsetExtractor
    {
        private readonly Document _doc;
        private readonly MEPCurve _mEPCurve;
        private readonly XYZ _baseConnectorPoint;
        private readonly double _offset;
        private XYZ _offsetDir;
        private readonly XYZ _startPoint;
        private readonly XYZ _endPoint;
        private readonly TransactionBuilder _transactionBuilder;
        private readonly List<Transform> _transforms;
        private readonly double _extrusionDist;
        private readonly XYZ _extrusionDir;

        /// <summary>
        /// Create a new instance of object to extract solid from <paramref name="mEPCurve"/>
        /// between <paramref name="startPoint"/> and <paramref name="endPoint"/>.
        /// <para>
        /// If <paramref name="startPoint"/> or <paramref name="endPoint"/> is null, end point of <paramref name="mEPCurve"/> will be set.
        /// </para>
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="offset"></param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        public SolidOffsetExtractor(MEPCurve mEPCurve, double offset, XYZ startPoint = null, XYZ endPoint = null)
        {
            _doc = mEPCurve.Document;
            _transactionBuilder = new TransactionBuilder(_doc);

            _mEPCurve = mEPCurve;
            var (con1, con2) = mEPCurve.GetMainConnectors();
            _baseConnectorPoint = con1.Origin;
            _offset = offset;
            _offsetDir = (_baseConnectorPoint- con2.Origin).Normalize();

            _startPoint = startPoint ?? con1.Origin;
            _endPoint = endPoint ?? con2.Origin;

            _extrusionDir = (_endPoint - _startPoint).Normalize();
            _extrusionDist = _startPoint.DistanceTo(_endPoint);

            _transforms = GetTransforms(_mEPCurve, _baseConnectorPoint, startPoint, endPoint);              
        }

        /// <summary>
        /// Extract <see cref="Solid"/> from <see cref="_mEPCurve"/> and place it between startPoin and endPoint.
        /// </summary>
        /// <returns></returns>
        public Solid Extract()
        {
            var solid = ElementUtils.GetSolid(_mEPCurve);
            List<Curve> faceCurves = GetFaceCurves(solid.Faces);

            //List<Curve> offsetCurves = GetOffsetCurves(faceCurves);
            faceCurves = _transforms.Any() ? GetTransformed(faceCurves) : faceCurves;
            //_transactionBuilder.Build(() => faceCurves.ForEach(curve => curve.Show(_doc)), "showcurves");

            //connect curves
            List<Line> lines = faceCurves.OfType<Line>().ToList();
            faceCurves = faceCurves.Where(obj => obj is not Line).ToList();

            List<Curve> connectedCurves = lines.Any() ?
                new LinesConnector(lines).Connect().Cast<Curve>().ToList() :
                new List<Curve>();

            faceCurves.AddRange(connectedCurves);
            //return null;
            return CreateExtrudedSolid(faceCurves);
        }


        #region PrivateMethods

        private List<Curve> GetTransformed(List<Curve> curves)
        {
            var transformFaceCurves = new List<Curve>();
            foreach (var curve in curves)
            {
                Curve transformedCurve = curve;
                foreach (var t in _transforms)
                {transformedCurve = transformedCurve.CreateTransformed(t);}
                transformFaceCurves.Add(transformedCurve);
            }
            return transformFaceCurves;
        }

        private List<Curve> GetFaceCurves(FaceArray faceArray)
        {
            var faceCurves = new List<Curve>();

            var face = faceArray.GetFace(_baseConnectorPoint);
            EdgeArray edgeArray1 = face.EdgeLoops.get_Item(0);
            for (int i = 0; i < 4; i++)
            {
                Edge edge = edgeArray1.get_Item(i);
                var curve = edge.AsCurve();
                curve = _offset == 0 ? curve : curve.CreateOffset(_offset, _offsetDir);
                faceCurves.Add(curve);
            }

            return faceCurves;
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

        private List<Transform> GetTransforms(MEPCurve mEPCurve, XYZ basePoint, XYZ startPoint, XYZ endPoint)
        {
            Basis sourceBasis = mEPCurve.GetBasis(basePoint);
            //_transactionBuilder.Build(() => sourceBasis.Show(_doc), "show basis");

            Line line = Line.CreateBound(startPoint, endPoint);
            var basisCoords = new List<XYZ>()
            { sourceBasis.X, sourceBasis.Y, sourceBasis.Z};

            var basisY = XYZUtils.GetPerpendicular(line.Direction, basisCoords).FirstOrDefault();
            Basis targetBasis = line.GetBasis(_startPoint, basisY);

            var transformModel = new BasisTransformBuilder(sourceBasis, targetBasis).Build();
            List<Transform> transforms = transformModel?.Transforms ?? new List<Transform>();           

            sourceBasis.Transform(transforms);

            var alignTransform = MEPCurveUtils.GetAlignTransform(mEPCurve, sourceBasis);
            if (alignTransform != null) { transforms.Add(alignTransform); }

            //_transactionBuilder.Build(() => sourceBasis.Show(_doc), "show transformed basis");

            return transforms;
        }

        #endregion

    }
}
