using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Basis;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Various.Bases;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// A new instance of object to extract solid.
    /// </summary>
    public class BestSolidOffsetExtractor
    {
        private readonly Document _doc;
        private readonly MEPCurve _mEPCurve;
        private readonly BasisXYZ _mcBasis;
        private List<Curve> _sourceFaceCurves;

        /// <summary>
        /// Create a new instance of object to extract solid from <paramref name="mEPCurve"/>.
        /// </summary>
        /// <param name="mEPCurve"></param>
        /// <param name="offset"></param>
        public BestSolidOffsetExtractor(MEPCurve mEPCurve, double offset)
        {
            _doc = mEPCurve.Document;
            _mEPCurve = mEPCurve;

            var (con1, con2) = mEPCurve.GetMainConnectors();
            var offsetDir = (con1.Origin - con2.Origin).Normalize();

            _mcBasis = mEPCurve.GetBasisXYZ(offsetDir, con1.Origin);

            var sourceSolid = ElementUtils.GetSolid(_mEPCurve);
            _sourceFaceCurves = GetFaceCurves(sourceSolid.Faces, _mcBasis, offset);
        }

        /// <summary>
        /// Set a new source basis.
        /// </summary>
        /// <param name="sourceBasis"></param>
        public void SetSource(Basis3d sourceBasis)
        {
            SourceBasis3d = sourceBasis;
            List<Transform> transforms = _mcBasis.ToBasis3d().GetTransforms(sourceBasis);
            _sourceFaceCurves = GetTransformed(_sourceFaceCurves, transforms);
        }

        /// <summary>
        /// Source basis.
        /// </summary>
        public ClassLib.VarUtils.Basis.Basis3d SourceBasis3d { get; private set; }

        /// <summary>
        /// Extract <see cref="Solid"/> from <see cref="_mEPCurve"/> and place it between <paramref name="startPoint"/> and <paramref name="startPoint"/>
        /// with transformations by <paramref name="targetBasis"/>.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="targetBasis"></param>
        /// <returns>
        /// Offsetted transformed <see cref="Solid"/>.
        /// </returns>  
        public Solid Extract(XYZ startPoint, XYZ endPoint, BasisXYZ targetBasis)
        {
            List<Transform> transforms = SourceBasis3d.GetTransforms(targetBasis.ToBasis3d());

            var vector =  startPoint - targetBasis.Origin;
            var translation = Transform.CreateTranslation(vector);
            transforms.Add(translation);

            var trCurves = GetTransformed(_sourceFaceCurves, transforms);

            var dir = (endPoint - startPoint).Normalize();
            return Extract(trCurves,dir , startPoint.DistanceTo(endPoint));
        }


        #region PrivateMethods


        private Solid Extract(List<Curve> faceCurves, XYZ extrusionDir, double extrusionDis)
        {
            //connect curves
            List<Line> lines = faceCurves.OfType<Line>().ToList();
            faceCurves = faceCurves.Where(obj => obj is not Line).ToList();

            List<Curve> connectedCurves = lines.Any() ?
                new LinesConnector(lines).Connect().Cast<Curve>().ToList() :
                new List<Curve>();

            faceCurves.AddRange(connectedCurves);
            return CreateExtrudedSolid(faceCurves, extrusionDir, extrusionDis);
        }

        private List<Curve> GetTransformed(List<Curve> curves, List<Transform> transforms)
        {
            var transformFaceCurves = new List<Curve>();
            foreach (var curve in curves)
            {
                Curve transformedCurve = curve;
                foreach (var t in transforms)
                { transformedCurve = transformedCurve.CreateTransformed(t); }
                transformFaceCurves.Add(transformedCurve);
            }
            return transformFaceCurves;
        }

        private List<Curve> GetFaceCurves(FaceArray faceArray, BasisXYZ sourceBasis, double offset)
        {
            var faceCurves = new List<Curve>();

            var face = faceArray.GetFace(sourceBasis.Origin);
            EdgeArray edgeArray1 = face.EdgeLoops.get_Item(0);
            for (int i = 0; i < 4; i++)
            {
                Edge edge = edgeArray1.get_Item(i);
                var curve = edge.AsCurve();
                curve = offset == 0 ? curve : curve.CreateOffset(offset, sourceBasis.X);
                faceCurves.Add(curve);
            }
          
            return faceCurves;
        }

        private Solid CreateExtrudedSolid(List<Curve> curves, XYZ extrusionDir, double extrusionDist)
        {
            var cloop = new CurveLoop();

            foreach (var curve in curves)
            { cloop.Append(curve); }
            IList<CurveLoop> loop = new List<CurveLoop>() { cloop };

            return GeometryCreationUtilities.CreateExtrusionGeometry(loop, extrusionDir, extrusionDist);
        }

        #endregion

    }
}
