using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.PathCreators;
using DS.RevitLib.Utils.Solids.Models;

namespace DS.RevitLib.Test
{
    internal class PathFinderTest
    {
        private readonly Document _doc;
        private readonly UIDocument _uIDoc;
        private readonly TransactionBuilder _trb;

        public PathFinderTest(Document doc, UIDocument uIDoc)
        {
            _doc = doc;
            _uIDoc = uIDoc;
            _trb = new TransactionBuilder(_doc);
        }

        public void Run()
        {
            XYZ startPoint;
            XYZ endPoint;

            ObjectSnapTypes snapTypes = ObjectSnapTypes.Endpoints | ObjectSnapTypes.Intersections;

            var ref1 = _uIDoc.Selection.PickObject(ObjectType.PointOnElement, "Select startPoint");
            MEPCurve mEPCurve1 = _doc.GetElement(ref1) as MEPCurve;
            var ref2 = _uIDoc.Selection.PickObject(ObjectType.PointOnElement, "Select endPoint");
            MEPCurve mEPCurve2 = _doc.GetElement(ref2) as MEPCurve;
            _uIDoc.Selection.SetElementIds(new List<ElementId> { mEPCurve1.Id });
            _uIDoc.RefreshActiveView();

            startPoint = ref1.GlobalPoint;
            endPoint = ref2.GlobalPoint;
            //startPoint = _uIDoc.Selection.PickPoint(snapTypes, "Select startPoint");
            //endPoint = _uIDoc.Selection.PickPoint(snapTypes, "Select endPoint");

            //Reference reference = _uIDoc.Selection.PickObject(ObjectType.Element, "Select MEPCurve1");
            //MEPCurve mEPCurve = _doc.GetElement(reference) as MEPCurve;
            (double width, double heigth) = MEPCurveUtils.GetWidthHeight(mEPCurve1);
            ElementUtils.GetPoints(mEPCurve1, out XYZ p11, out XYZ p12, out XYZ c1);

            startPoint.Show(_doc, 1);
            endPoint.Show(_doc, 1);
            _uIDoc.RefreshActiveView();

            List<XYZ> path = IvanovPathFinderTest(mEPCurve1, mEPCurve2, startPoint, endPoint);

            //List<XYZ> path = FindPath1(mEPCurve1, mEPCurve2, startPoint, endPoint, width, heigth);

            _trb.Build(() => ShowPath(path), "show path");
        }

        //private List<XYZ> FindPath1(MEPCurve mEPCurve1, MEPCurve mEPCurve2, XYZ startPoint, XYZ endPoint, double width, double heigth)
        //{
        //    //создаем опции поиска
        //    //параметр в конструкторе это Ширина отвода от оси до грани
        //    //исходя из этого параметра будет подбираться минимальный шаг поиска так, что
        //    //минимальная длина прямого участка 50 мм + 2 * Ширина отвода
        //    var exceptions = new List<int> { mEPCurve1.Id.IntegerValue, mEPCurve2.Id.IntegerValue };
        //    var options = new FinderOptions(exceptions);

        //    //класс анализирует геометрию
        //    var geometryDocuments = GeometryDocuments.Create(_doc, options);

        //    //класс для поиска пути
        //    var finder = new PathFinderToOnePoint(startPoint, endPoint,
        //                 heigth, width, geometryDocuments, options);

        //    //ищем путь
        //    List<XYZ> path = new List<XYZ>();
        //    Task<List<XYZ>> task = finder.FindPath(new CancellationTokenSource().Token);
        //    task.Wait();
        //    path = task.Result;

        //    if (path == null)
        //    {
        //        Debug.Print("не удалось найти путь");
        //    }

        //    //объединяем прямые последовательные участки пути в один сегмент
        //    return Optimizer.MergeStraightSections(path, options);
        //}


        private List<XYZ> IvanovPathFinderTest(MEPCurve mEPCurve1, MEPCurve mEPCurve2, XYZ startPoint, XYZ endPoint)
        {
            var mEPSystemBuilder = new SimpleMEPSystemBuilder(mEPCurve1);
            var sourceMEPModel = mEPSystemBuilder.Build();

            var mEPCurveModel = new MEPCurveModel(mEPCurve1, new SolidModel(ElementUtils.GetSolid(mEPCurve1)));

            double elbowRadius = new ElbowRadiusCalc(mEPCurveModel).GetRadius(90.DegToRad()).Result;
           
            var pathFinder = new PathFindCreator().Create(_doc, elbowRadius, XYZ.BasisX,
                mEPCurve1.Height, mEPCurve1.Width);

            var elementsToDelete = new List<Element>() { mEPCurve1, mEPCurve2 };
            pathFinder.ExceptionElements = elementsToDelete.Select(obj => obj.Id).ToList();

            return _trb.Build(() => pathFinder.CreateAsync(startPoint, endPoint), "find Path").Result;
        }

        private void ShowPath(List<XYZ> path)
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
