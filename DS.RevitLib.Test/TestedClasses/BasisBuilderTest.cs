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
            Run();
        }

        private void Run()
        {
            _selector = new PointSelector(_uiDoc) { AllowLink = true };
            _visualisator = new XYZVisualizator(_uiDoc, _labelSize);
            _point3dVisualisator =
               new Point3dVisualisator(_uiDoc, null, _labelSize, null, true);
            var transforms = GetTransfrom();
            if (transforms == null) { return; }

            //var xYZ = new XYZ(1, 0, 0);
            //var xYZ = new XYZ(0, 1, 0);
            //var xYZ = new XYZ(0, 0, 1);
            //var xYZ = new XYZ(1, 1, 0);
            //var xYZ = new XYZ(1, 0, 1);
            //var xYZ = new XYZ(0, 1, 1);
            var xYZ = new XYZ(1, 1, 1);
            TryTransform(transforms, xYZ);


        }

        private List<Autodesk.Revit.DB.Transform> GetTransfrom()
        {

            //Point3d point = new Point3d(-1, 0, 0);
            //_point3dVisualisator.Show(point);

            //Vector3d diagonal = new Vector3d(point.X, point.Y, point.Z);
            //var movePoint = origin + diagonal;
            //_point3dVisualisator.ShowVector(origin, movePoint);

            (Point3d initialOrigin, Vector3d initialBasisX, Vector3d initialBasisY, Vector3d initialBasisZ) = GetOriginBasis();
            //(Point3d initialOrigin, Vector3d initialBasisX, Vector3d initialBasisY, Vector3d initialBasisZ) = GetInitialBasis();
            //_point3dVisualisator.Show(initialOrigin);
            //ShowBases(initialOrigin, initialBasisX, initialBasisY, initialBasisZ);

            Debug.Assert(Vector3d.AreRighthanded(initialBasisX, initialBasisY, initialBasisZ), "AreRighthanded");
            Debug.Assert(Vector3d.AreOrthonormal(initialBasisX, initialBasisY, initialBasisZ), "AreOrthonormal");


            var targetOrigin = Point3d.Origin;
            Vector3d stargetBasisX = new Vector3d(1, 1, 1);
            //Vector3d stargetBasisX = new Vector3d(0, 1, 1);
            //Vector3d stargetBasisX = new Vector3d(1, 0, 1);
            //Vector3d stargetBasisX = new Vector3d(1, 1, 0);
            //Vector3d stargetBasisX = new Vector3d(0, 0, 1);
            //Vector3d stargetBasisX = new Vector3d(0, 1, 0);
            //Vector3d stargetBasisX = GetTargetBasisX(out Point3d targetOrigin);
            (Vector3d targetBasisX, Vector3d targetBasisY, Vector3d targetBasisZ) = GetTargetBasis(stargetBasisX, initialBasisX, initialBasisY, initialBasisZ);
            ShowBases(targetOrigin, targetBasisX, targetBasisY, targetBasisZ);

            Debug.WriteLine("");
            Debug.WriteLine($"origin: {targetOrigin}");
            Debug.WriteLine($"basisX: {targetBasisX}");
            Debug.WriteLine($"basisY: {targetBasisY}");
            Debug.WriteLine($"basisZ: {targetBasisZ}");

            var rigth = Vector3d.AreRighthanded(targetBasisX, targetBasisY, targetBasisZ);
            var orthnorm = Vector3d.AreOrthonormal(targetBasisX, targetBasisY, targetBasisZ);
            Debug.Assert(rigth, "AreRighthanded");
            Debug.Assert(orthnorm, "AreOrthonormal");
            //Debug.Assert(basisY.IsParallelTo(initialBasisY) == 1 || basisY.IsParallelTo(initialBasisX) == 1, "BasisY direction failure");
            if (!rigth || !orthnorm) { return null; }

            Transform transform = Transform.ChangeBasis(
              initialBasisX, initialBasisY, initialBasisZ,
              targetBasisX, targetBasisY, targetBasisZ);

            var transforms = Decomposite(transform,
                initialOrigin, initialBasisX, initialBasisY, initialBasisZ,
                targetOrigin, targetBasisX, targetBasisY, targetBasisZ);

            return transforms;
        }

        private List<Autodesk.Revit.DB.Transform> Decomposite(Transform transform,
            Point3d initialOrigin, Vector3d initialBasisX, Vector3d initialBasisY, Vector3d initialBasisZ,
            Point3d targetOrigin, Vector3d targetBasisX, Vector3d targetBasisY, Vector3d targetBasisZ)
        {
            var xYZTransforms = new List<Autodesk.Revit.DB.Transform>();

            transform.TryGetInverse(out Transform inverseTransform);

            transform.GetEulerZYZ(out double alpha1, out double beta1, out double gamma1);

            double alpha = alpha1.RadToDeg();
            double beta = beta1.RadToDeg();
            double gamma = gamma1.RadToDeg();

            XYZ origin1 = initialOrigin.ToXYZ();
            XYZ initpx = initialBasisX.ToXYZ();
            XYZ initpy = initialBasisY.ToXYZ();
            XYZ initpz = initialBasisZ.ToXYZ();

            XYZ px = initialBasisX.ToXYZ();
            XYZ py = initialBasisY.ToXYZ();
            XYZ pz = initialBasisZ.ToXYZ();

            var xYZTransform1 = Autodesk.Revit.DB.Transform.CreateRotationAtPoint(initpz, -alpha1, origin1);
            xYZTransforms.Add(xYZTransform1);
            px = xYZTransform1.OfVector(initialBasisX.ToXYZ());
            py = xYZTransform1.OfVector(initialBasisY.ToXYZ());
            pz = xYZTransform1.OfVector(initialBasisZ.ToXYZ());

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

        private (Point3d origin, Vector3d initialBasisX, Vector3d initialBasisY, Vector3d initialBasisZ) GetOriginBasis()
        {
            return (Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis);
        }

        private (Point3d origin, Vector3d initialBasisX, Vector3d initialBasisY, Vector3d initialBasisZ) GetInitialBasis()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element1");
            var mc1 = _doc.GetElement(reference) as MEPCurve;
            var xYZBasis = mc1.GetBasis();

            var origin = xYZBasis.Point.ToPoint3d();
            var initialBasisX = xYZBasis.X.ToVector3d();
            var initialBasisY = xYZBasis.Y.ToVector3d();
            var initialBasisZ = xYZBasis.Z.ToVector3d();
            return (origin, initialBasisX, initialBasisY, initialBasisZ);
        }

        private (Vector3d basisX, Vector3d basisY, Vector3d basisZ) GetTargetBasis(Vector3d targetBasisX,
            Vector3d initialBasisX, Vector3d initialBasisY, Vector3d initialBasisZ)
        {
            Vector3d basisX = targetBasisX;

            double aTolerance = 3.DegToRad();
            Vector3d basisY;
            var xCross = Vector3d.CrossProduct(basisX, initialBasisX);
            //if (xCross.IsParallelTo(initialBasisZ, tolerance) == 1) { basisY = Vector3d.CrossProduct(initialBasisZ, basisX); }
            //else if (xCross.IsParallelTo(initialBasisY, tolerance) == 1) { basisY = initialBasisY; }
            if (basisX.IsPerpendicularTo(initialBasisY, aTolerance)) { basisY = initialBasisY; }
            else if (basisX.IsPerpendicularTo(initialBasisZ, aTolerance)) { basisY = Vector3d.CrossProduct(initialBasisZ, basisX); ; }
            else if (basisX.IsPerpendicularTo(initialBasisX, aTolerance)) { basisY = initialBasisX; }
            else { basisY = xCross; }

            Vector3d basisZ = Vector3d.CrossProduct(basisX, basisY);

            basisX = Vector3d.Divide(basisX, basisX.Length);
            basisY = Vector3d.Divide(basisY, basisY.Length);
            basisZ = Vector3d.Divide(basisZ, basisZ.Length);

            return (basisX, basisY, basisZ);
            //return (basisX.Round(_tolerance), basisY.Round(_tolerance), basisZ.Round(_tolerance));
        }


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
