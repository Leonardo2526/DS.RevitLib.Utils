using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils.Basis;
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
using DS.RevitLib.Utils.Various.Bases;
using DS.RevitLib.Utils;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class BestSolidOffsetExtractorTest
    {

        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private PointSelector _selector;
        private XYZVisualizator _visualisator;
        private Point3dVisualisator _point3dVisualisator;
        private readonly TransactionBuilder _trb;
        private readonly double _labelSize = 100.MMToFeet();
        private readonly int _tolerance = 9;

        public BestSolidOffsetExtractorTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = uidoc.Document;
            _selector = new PointSelector(_uiDoc) { AllowLink = true };
            _visualisator = new XYZVisualizator(_uiDoc, _labelSize);
            _point3dVisualisator =
               new Point3dVisualisator(_uiDoc, null, _labelSize, null, true);
            _trb = new TransactionBuilder(_doc);
            Run();
        }

        private void Run()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element to get target basis");
            var mc1 = _doc.GetElement(reference) as MEPCurve;
            var offset = 100.MMToFeet();

            var extractor = new BestSolidOffsetExtractor(mc1, offset);

            Vector3d stargetBasisX = GetTargetBasisX(out Point3d targetOrigin, out XYZ startPoint, out XYZ endPoint);
            var targetBasis = extractor.SourceBasis3d.GetBasis(stargetBasisX);
            targetBasis.Origin = targetOrigin;
            targetBasis.Show(_uiDoc);
            Debug.Assert(targetBasis.IsRighthanded(), "AreRighthanded");

           var targetSolid = extractor.Extract(startPoint, endPoint, targetBasis.ToXYZ());
            _trb.Build(() =>
            {
                targetSolid.ShowShape(_doc);
            }, "show targetSolid");
        }

        private Vector3d GetTargetBasisX(out Point3d targetOrigin, out XYZ startPoint, out XYZ endPoint)
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select element to get target basis");
            var mc1 = _doc.GetElement(reference) as MEPCurve;

            targetOrigin = mc1.GetCenterPoint().ToPoint3d();
            //targetOrigin =mc1.GetCenterPoint().ToPoint3d().Round(_tolerance);

            var (con1, con2) = mc1.GetMainConnectors();

            startPoint = con1.Origin;
            endPoint = con2.Origin;

            return MEPCurveUtils.GetDirection(mc1).ToVector3d();
        }
    }
}
