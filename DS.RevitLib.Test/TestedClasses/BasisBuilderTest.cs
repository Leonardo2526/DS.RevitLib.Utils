using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Points;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry.Points;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Various.Selections;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;
using Transform = Rhino.Geometry.Transform;
using DS.ClassLib.VarUtils.Basis;
using DS.RevitLib.Utils.Various.Bases;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class BasisBuilderTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private PointSelector _selector;
        private XYZVisualizator _visualisator;
        private Point3dVisualisator _point3dVisualisator;
        private readonly double _labelSize = 100.MMToFeet();
        private readonly int _tolerance = 9;

        public BasisBuilderTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = uidoc.Document;
            _selector = new PointSelector(_uiDoc) { AllowLink = true };
            _visualisator = new XYZVisualizator(_uiDoc, _labelSize);
            _point3dVisualisator =
               new Point3dVisualisator(_uiDoc, null, _labelSize, null, true);
            RunMulti();
            //Run();
        }

        private void Run()
        {
            //var transforms = GetTransfroms();
            var transforms = GetMEPCurveTransfroms();
            if (transforms == null) { return; }

            //var xYZ = new XYZ(1, 0, 0);
            //var xYZ = new XYZ(0, 1, 0);
            //var xYZ = new XYZ(0, 0, 1);
            //var xYZ = new XYZ(1, 1, 0);
            //var xYZ = new XYZ(1, 0, 1);
            //var xYZ = new XYZ(0, 1, 1);
            //var xYZ = new XYZ(1, 1, 1);
            //TryTransform(transforms, xYZ);
        }

        private void RunMulti()
        {
            Transform transform1 = GetMEPCurveTransfrom(out Basis3d sourceBasis1, out Basis3d targetBasis1);
            XYZ testPoint = (sourceBasis1.Origin + sourceBasis1.X).ToXYZ();
            testPoint.Show(_doc);

            Transform transform2 = GetMEPCurveTransfrom(out Basis3d sourceBasis2, out Basis3d targetBasis2);
            List<Autodesk.Revit.DB.Transform> transforms = sourceBasis1.GetTransforms(targetBasis2);
            //var transform = transform1 * transform2;

            var trPoint = testPoint.Transform(transforms);
            trPoint.Show(_doc);
        }

        private List<Autodesk.Revit.DB.Transform> GetTransfroms()
        {
            var sourceBasis = new Basis3d(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis);
            sourceBasis.Show(_uiDoc);
            Debug.Assert(sourceBasis.IsRighthanded(), "AreRighthanded");

            var targetOrigin = new Point3d(5, -6, 10);
            Vector3d stargetBasisX = new Vector3d(1, 1, 1);
            Basis3d targetBasis = sourceBasis.GetBasis(stargetBasisX);
            targetBasis.Origin = targetOrigin;
            targetBasis.Show(_uiDoc);
            Debug.Assert(targetBasis.IsRighthanded(), "AreRighthanded");

            Debug.WriteLine("");
            Debug.WriteLine($"origin: {targetOrigin}");
            Debug.WriteLine($"basisX: {targetBasis.X}");
            Debug.WriteLine($"basisY: {targetBasis.Y}");
            Debug.WriteLine($"basisZ: {targetBasis.Z}");

            return sourceBasis.GetTransforms(targetBasis); ;
        }

        private List<Autodesk.Revit.DB.Transform> GetMEPCurveTransfroms()
        {
            var sourceBasis = GetSourceBasis();
            sourceBasis.Show(_uiDoc);
            Debug.Assert(sourceBasis.IsRighthanded(), "AreRighthanded");

            Vector3d stargetBasisX = GetTargetBasisX(out Point3d targetOrigin);
            Basis3d targetBasis = sourceBasis.GetBasis(stargetBasisX);
            targetBasis.Origin = targetOrigin;
            targetBasis.Show(_uiDoc);
            Debug.Assert(targetBasis.IsRighthanded(), "AreRighthanded");

            Debug.WriteLine("");
            Debug.WriteLine($"origin: {targetOrigin}");
            Debug.WriteLine($"basisX: {targetBasis.X}");
            Debug.WriteLine($"basisY: {targetBasis.Y}");
            Debug.WriteLine($"basisZ: {targetBasis.Z}");

            return sourceBasis.GetTransforms(targetBasis);
        }

        private Transform GetMEPCurveTransfrom(out Basis3d sourceBasis, out Basis3d targetBasis)
        {
            sourceBasis = GetSourceBasis();
            sourceBasis.Show(_uiDoc);
            Debug.Assert(sourceBasis.IsRighthanded(), "AreRighthanded");

            Vector3d stargetBasisX = GetTargetBasisX(out Point3d targetOrigin);
            targetBasis = sourceBasis.GetBasis(stargetBasisX);
            targetBasis.Origin = targetOrigin;
            targetBasis.Show(_uiDoc);
            Debug.Assert(targetBasis.IsRighthanded(), "AreRighthanded");

            Debug.WriteLine("");
            Debug.WriteLine($"origin: {targetOrigin}");
            Debug.WriteLine($"basisX: {targetBasis.X}");
            Debug.WriteLine($"basisY: {targetBasis.Y}");
            Debug.WriteLine($"basisZ: {targetBasis.Z}");

            return sourceBasis.GetTransform(targetBasis);
        }

        private List<Autodesk.Revit.DB.Transform> Decomposite(Transform transform, Basis3d initialBasis, Basis3d targetBasis)
        {
            var xYZTransforms = new List<Autodesk.Revit.DB.Transform>();

            transform.TryGetInverse(out Transform inverseTransform);

            transform.GetEulerZYZ(out double alpha1, out double beta1, out double gamma1);

            double alpha = alpha1.RadToDeg();
            double beta = beta1.RadToDeg();
            double gamma = gamma1.RadToDeg();

            XYZ origin1 = initialBasis.Origin.ToXYZ();
            XYZ initpx = initialBasis.X.ToXYZ();
            XYZ initpy = initialBasis.Y.ToXYZ();
            XYZ initpz = initialBasis.Z.ToXYZ();

            XYZ px = initialBasis.X.ToXYZ();
            XYZ py = initialBasis.Y.ToXYZ();
            XYZ pz = initialBasis.Z.ToXYZ();          

            var xYZTransform1 = Autodesk.Revit.DB.Transform.CreateRotationAtPoint(initpz, -alpha1, origin1);
            xYZTransforms.Add(xYZTransform1);
            px = xYZTransform1.OfVector(initialBasis.X.ToXYZ());
            py = xYZTransform1.OfVector(initialBasis.Y.ToXYZ());
            pz = xYZTransform1.OfVector(initialBasis.Z.ToXYZ());

            var xYZTransform2 = Autodesk.Revit.DB.Transform.CreateRotationAtPoint(initpy, -beta1, origin1);
            xYZTransforms.Add(xYZTransform2);
            px = xYZTransform2.OfVector(px);
            py = xYZTransform2.OfVector(py);
            pz = xYZTransform2.OfVector(pz);

            var xYZTransform3 = Autodesk.Revit.DB.Transform.CreateRotationAtPoint(initpz, -gamma1, origin1);
            xYZTransforms.Add(xYZTransform3);
            px = xYZTransform3.OfVector(px);
            py = xYZTransform3.OfVector(py);
            pz = xYZTransform3.OfVector(pz);

            var translation  = Autodesk.Revit.DB.Transform.CreateTranslation((targetBasis.Origin - initialBasis.Origin).ToXYZ());
            xYZTransforms.Add(translation);

            return xYZTransforms;
            //var dec = transform.DecomposeRigid(out Vector3d translation, out Transform rotation, _tolerance);
        }

        private XYZ TryTransform(List<Autodesk.Revit.DB.Transform> transforms, XYZ point)
        {
            XYZ xYZ = new XYZ(point.X, point.Y, point.Z);
            foreach (var transform in transforms)
            {
                xYZ = transform.OfPoint(xYZ);
            }

            xYZ.Show(_doc);
            return xYZ;
        }

        private Vector3d GetTargetBasisX(out Point3d targetOrigin)
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element to get target basis");
            var mc1 = _doc.GetElement(reference) as MEPCurve;

            targetOrigin = mc1.GetCenterPoint().ToPoint3d();
            //targetOrigin =mc1.GetCenterPoint().ToPoint3d().Round(_tolerance);

            return MEPCurveUtils.GetDirection(mc1).ToVector3d();
        }

        private Basis3d GetSourceBasis()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var mc1 = _doc.GetElement(reference) as MEPCurve;
            var xYZBasis = mc1.GetBasis();

            var origin = xYZBasis.Point.ToPoint3d();
            var initialBasisX = xYZBasis.X.ToVector3d();
            var initialBasisY = xYZBasis.Y.ToVector3d();
            var initialBasisZ = xYZBasis.Z.ToVector3d();
            return new Basis3d(origin, initialBasisX, initialBasisY, initialBasisZ);
        }

        //private (Vector3d basisX, Vector3d basisY, Vector3d basisZ) GetTargetBasis(Vector3d targetBasisX,
        //    Vector3d initialBasis.X, Vector3d initialBasis.Y, Vector3d initialBasis.Z)
        //{
        //    Vector3d basisX = targetBasisX;

        //    double aTolerance = 3.DegToRad();
        //    Vector3d basisY;
        //    var xCross = Vector3d.CrossProduct(basisX, initialBasis.X);
        //    //if (xCross.IsParallelTo(initialBasis.Z, tolerance) == 1) { basisY = Vector3d.CrossProduct(initialBasis.Z, basisX); }
        //    //else if (xCross.IsParallelTo(initialBasis.Y, tolerance) == 1) { basisY = initialBasis.Y; }
        //    if (basisX.IsPerpendicularTo(initialBasis.Y, aTolerance)) { basisY = initialBasis.Y; }
        //    else if (basisX.IsPerpendicularTo(initialBasis.Z, aTolerance)) { basisY = Vector3d.CrossProduct(initialBasis.Z, basisX); ; }
        //    else if (basisX.IsPerpendicularTo(initialBasis.X, aTolerance)) { basisY = initialBasis.X; }
        //    else { basisY = xCross; }

        //    Vector3d basisZ = Vector3d.CrossProduct(basisX, basisY);

        //    basisX = Vector3d.Divide(basisX, basisX.Length);
        //    basisY = Vector3d.Divide(basisY, basisY.Length);
        //    basisZ = Vector3d.Divide(basisZ, basisZ.Length);

        //    return (basisX, basisY, basisZ);
        //    //return (basisX.Round(_tolerance), basisY.Round(_tolerance), basisZ.Round(_tolerance));
        //}


        private void ShowVector()
        {
            var element = _selector.Pick($"Укажите точку 1.");
            ConnectionPoint connectionPoint1 = new ConnectionPoint(element, _selector.Point);
            if (connectionPoint1 == null) { return; }

            element = _selector.Pick($"Укажите точку 2.");
            ConnectionPoint connectionPoint2 = new ConnectionPoint(element, _selector.Point);
            if (connectionPoint2 == null) { return; }

            var p1 = connectionPoint1.Point; var p2 = connectionPoint2.Point;


            _visualisator.ShowVector(p1, p2);
        }

        private void ShowBases()
        {
            var element = _selector.Pick($"Укажите точку базиса.");
            ConnectionPoint connectionPoint1 = new ConnectionPoint(element, _selector.Point);
            if (connectionPoint1 == null) { return; }

            _visualisator.ShowVectorByDirection(connectionPoint1.Point, XYZ.BasisX);
            _visualisator.ShowVectorByDirection(connectionPoint1.Point, XYZ.BasisY);
            _visualisator.ShowVectorByDirection(connectionPoint1.Point, XYZ.BasisZ);
        }

        private void ShowBases(Point3d origin, Vector3d basisX, Vector3d basisY, Vector3d basisZ)
        {
            _point3dVisualisator.LabelSize = 200.MMToFeet();
            _point3dVisualisator.ShowVector(origin, origin + basisX);

            _point3dVisualisator.LabelSize = 100.MMToFeet();
            _point3dVisualisator.ShowVector(origin, origin + basisY);

            _point3dVisualisator.LabelSize = 20.MMToFeet();
            _point3dVisualisator.ShowVector(origin, origin + basisZ);

            _point3dVisualisator.LabelSize = 100.MMToFeet();
        }
    }
}
