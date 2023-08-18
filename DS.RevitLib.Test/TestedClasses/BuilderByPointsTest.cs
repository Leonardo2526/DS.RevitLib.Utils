using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.PathCreators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test
{
    internal class BuilderByPointsTest
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private MEPCurve _mc1;

        public BuilderByPointsTest(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
        }

        public void Run()
        {
            var transactionBuilder = new TransactionBuilder(_doc);

            List<XYZ> points = GetVerticalPoints();
            //transactionBuilder.Build(() => ShowPath(points), "ShowPath");

            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            _mc1 = _doc.GetElement(reference) as MEPCurve;
            var mc = _doc.GetElement(reference);
        
            var builder = new BuilderByPoints(_mc1, points);
            builder.BuildSystem(transactionBuilder);

            //var builder = new BuilderByPointsTransactions(mEPCurve, points);
            //transactionBuilder.Build(() => builder.BuildMEPCurves(), "Create MEPSystem");
            //MEPCurvesModel mEPElementsModel = null;
            //transactionBuilder.Build(() => mEPElementsModel = builder.BuildMEPCurves(), "Create MEPSystem by path");
            //transactionBuilder.Build(() => mEPElementsModel.RefineDucts(_mc1), "RectangularFixing");
            //transactionBuilder.Build(() => mEPElementsModel = mEPElementsModel.WithElbows(), "Insert elbows by path");
        }

        private List<XYZ> GetHorisontalPoints()
        {
            double step = 3;

            var points = new List<XYZ>() { new XYZ(0, 0, 0) };
            for (int i = 0; i < 10; i++)
            {
                XYZ point1 = new(points.Last().X + step, 0, 0);
                points.Add(point1);
                XYZ point2 = new(points.Last().X + step, points.Last().Y + step, 0);
                points.Add(point2);
            }

            return points;
        }

        private List<XYZ> GetVerticalPoints()
        {
            double step = 3;

            var points = new List<XYZ>() { new XYZ(0, 0, 0) };
            for (int i = 0; i < 10; i++)
            {
                XYZ point1 = new(0, 0, points.Last().Z + step);
                points.Add(point1);
                XYZ point2 = new(points.Last().X + step, 0, points.Last().Z + step);
                points.Add(point2);
            }

            return points;
        }

        private List<XYZ> GetPoints90_1()
        {
            double step = 3;
            var points = new List<XYZ>
            {
                new XYZ(0, 0, 0), new XYZ(step, 0, 0),
                new XYZ(step, step, 0), new XYZ(step * 2, step, 0),
                new XYZ(step * 2, step * 2, 0), new XYZ(step * 3, step * 2, 0),
                new XYZ(step * 3, step * 3, 0), new XYZ(step * 4, step * 3, 0),
            };
            return points;
        }

        private List<XYZ> GetPoints45_1()
        {
            double step = 5;
            var points = new List<XYZ>
            {
                new XYZ(0, 0, 0), new XYZ(step, step, 0),
                new XYZ(step * 2, step, 0), new XYZ(step * 3, step * 2, 0),
                new XYZ(step * 4, step * 2, 0), new XYZ(step * 5, step * 3, 0),
                new XYZ(step * 6, step * 3, 0), new XYZ(step * 7, step * 4, 0),
            };

            return points;
        }


        public List<XYZ> FindPath()
        {
            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element1");
            _mc1 = _doc.GetElement(reference) as MEPCurve;
            var (con11, con12) = ConnectorUtils.GetMainConnectors(_mc1);

            reference = _uidoc.Selection.PickObject(ObjectType.Element, "Select element2");
            var mc2 = _doc.GetElement(reference) as MEPCurve;
            var (con21, con22) = ConnectorUtils.GetMainConnectors(mc2);
            double midDistPoints = 500.mmToFyt2();
            var finder = new PathFindCreator();
                //Create(_mc1.GetCenterLine(), mc2.GetCenterLine(), midDistPoints, midDistPoints * 5, 90, midDistPoints * 3);
            return finder.CreateAsync(con11.Origin, con21.Origin).Result;
        }

        public void ShowPath(List<XYZ> path)
        {
            var mcreator = new ModelCurveCreator(_doc);
            for (int i = 0; i < path.Count - 1; i++)
            {
                mcreator.Create(path[i], path[i + 1]);
                var line = Line.CreateBound(path[i], path[i + 1]);
            }
        }

    }
}
