using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Visualisators;

namespace DS.RevitLib.Test
{
    internal class ArcCreationTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly TransactionBuilder _transactionBuilder;
        private List<XYZ> _linePoints;
        private List<Line> _lines;
        private double _offset;

        public ArcCreationTest(Document doc, UIDocument uiDoc)
        {
            Debug.IndentLevel = 1;
            _uiDoc = uiDoc;
            _doc = doc;
            _transactionBuilder = new TransactionBuilder(doc);
            _linePoints = new List<XYZ>();
            _lines= new List<Line>();
            _offset = 1;
        }

        public void Run()
        {
            Reference reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select line1");
            var circle1 = GetCircle(reference);
            GeometryElementsUtils.Show(circle1, _doc);
            //var points1 = circle1.Tessellate();
            //ShowCirclePoints(circle1);

            reference = _uiDoc.Selection.PickObject(ObjectType.Element, "Select line2");
            var circle2 = GetCircle(reference);
            GeometryElementsUtils.Show(circle2, _doc);
            //var points2 = circle1.Tessellate();
            //ShowCirclePoints(circle2);

            //Show line points
            foreach (var point in _linePoints)
            {
                point.ShowWithTransaction(_doc);
            }


            //Get and show bb
            var bb = ElementUtils.GetBoundingBox(_lines, _offset);
            bb.Min.ShowWithTransaction(_doc);
            bb.Max.ShowWithTransaction(_doc);
            _transactionBuilder.Build(() =>
            {
                var visualizator = new BoundingBoxVisualisator(bb, _doc);
                visualizator.Show();
            }, "show BoundingBox");
        }

        public void RunByPoint()
        {
            var points = new List<XYZ>();

            XYZ point1 = _uiDoc.Selection.PickPoint();
            point1.ShowWithTransaction(_doc);
            _uiDoc.RefreshActiveView();

            XYZ point2 = _uiDoc.Selection.PickPoint();
            point2.ShowWithTransaction(_doc);
            _uiDoc.RefreshActiveView();

            XYZ point3 = _uiDoc.Selection.PickPoint();
            point3.ShowWithTransaction(_doc);
            _uiDoc.RefreshActiveView();

            points.Add(point1);
            points.Add(point2);
            points.Add(point3);

            //Get and show bb
            var bb = ElementUtils.GetBoundingBox(points, _offset);
            bb.Min.ShowWithTransaction(_doc);
            bb.Max.ShowWithTransaction(_doc);
            _transactionBuilder.Build(() =>
            {
                var visualizator = new BoundingBoxVisualisator(bb, _doc);
                visualizator.Show();
            }, "show BoundingBox");
        }

        private Arc GetCircle(Reference reference)
        {
            var checkedElement = _doc.GetElement(reference) as ModelCurve;
            Line line = checkedElement.GeometryCurve as Line;
            _lines.Add(line);

            var p1 = line.GetEndPoint(0);
            var neighbourPoint = _linePoints.Where(obj => (obj - p1).IsZeroLength());
            if (!neighbourPoint.Any())
            {
                _linePoints.Add(p1);
            }

            var p2 = line.GetEndPoint(1);
            neighbourPoint = _linePoints.Where(obj => (obj - p2).IsZeroLength());
            if (!neighbourPoint.Any())
            {
                _linePoints.Add(p2);
            }
            XYZ dir = line.Direction;
            XYZ basePoint = line.GetEndPoint(0);
            return GeometryElementsUtils.CreateCircle(basePoint, dir, _offset);
        }

        private void ShowCirclePoints(Arc arc)
        {
            var offsetPoints1 = arc.Tessellate();
            foreach (var point in offsetPoints1)
            {
                point.ShowWithTransaction(_doc);
            }
        }
    }
}
