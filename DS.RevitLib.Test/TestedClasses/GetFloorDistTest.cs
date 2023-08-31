using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.TransactionCommitter;
using DS.RevitLib.Utils.Various.Selections;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class GetFloorDistTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ITransactionFactory _trf;

        List<BuiltInCategory> _exludedCathegories = new List<BuiltInCategory>()
        { };

        public GetFloorDistTest(UIDocument uidoc)
        {
            _uiDoc = uidoc;
            _doc = uidoc.Document;
            _trf = new ContextTransactionFactory(_doc);
            Run();
        }

        public void Run()
        {
            XYZ point = SelectPoint();
            var floorDist = point.GetDistanceToFloor(_doc);
            var ceilingDist = point.GetDistanceToCeiling(_doc);
            Debug.WriteLine(floorDist);
            Debug.WriteLine(ceilingDist);

            if (floorDist != double.PositiveInfinity)
            { Debug.WriteLine(floorDist.FeetToMM()); }

            if (ceilingDist != double.PositiveInfinity)
            { Debug.WriteLine(ceilingDist.FeetToMM()); }
        }

        public void Run1()
        {
            XYZ point = SelectPoint();

            var outline = GetOutline(point);

            (var docElements, var linkElementsDict) = new ElementsExtractor(_doc, _exludedCathegories, outline).GetAll();

            var floors = new List<Element>();
            var docFloors = docElements.Where(el => el is Floor).ToList();
            floors.AddRange(docFloors);
            foreach (var kv in linkElementsDict)
            {
                var elems = kv.Value;
                foreach (var e in elems)
                {
                    if (e is Floor)
                    {
                        floors.Add(e);
                    }
                }
            }

            var line = Line.CreateBound(new XYZ(point.X, point.Y, point.Z), new XYZ(point.X, point.Y, point.Z - 50));
            var opt = new SolidCurveIntersectionOptions() { ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside };

            //var intersects = docFloors.Select(x =>
            //    new
            //    {
            //        intersectResult = x.Solid().IntersectWithCurve(line, opt),
            //        gd = x
            //    }).OrderBy(x => x.intersectResult?.GetCurveSegment(0).Length).FirstOrDefault();
            //var intersectionCurve = intersects?.intersectResult?.FirstOrDefault();
            //Debug.WriteLine(intersectionCurve.Length);

            var intersections = new List<SolidCurveIntersection>();
            foreach (var f in docFloors)
            {
                var p1 = f.GetCenterPoint();
                var c = f.Solid().ComputeCentroid();
                var intersectResult = f.Solid().IntersectWithCurve(line, opt);
                if (intersectResult.SegmentCount > 0)
                {
                    intersections.Add(intersectResult);
                    //Debug.WriteLine(intersectResult.GetCurveSegment(0).Length);
                }
            }

            intersections.OrderBy(x => x.GetCurveSegment(0).Length);
            //foreach (var intersection in intersections)
            //{
            //    Debug.WriteLine(Math.Round(intersection.GetCurveSegment(0).Length.FeetToMM(), 3));
            //}

            Debug.WriteLine(Math.Round(intersections.FirstOrDefault().GetCurveSegment(0).Length.FeetToMM(), 3));

        }

        private XYZ SelectPoint()
        {
            var selector = new PointSelector(_uiDoc) { AllowLink = false };
            var element = selector.Pick($"Укажите точку присоединения на элементе.");
            XYZ point = selector.Point;
            return point;
        }

        private Outline GetOutline(XYZ point)
        {
            var vector = new XYZ(1, 1, 30);
            return new Outline(point - vector, point + vector);
        }


    }
}
