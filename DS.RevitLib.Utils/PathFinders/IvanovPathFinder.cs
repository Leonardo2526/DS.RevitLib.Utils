using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.Transactions;
using PathFinderLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.PathFinders
{
    /// <summary>
    /// An object that represents finder to get path.
    /// </summary>
    public class IvanovPathFinder : IPathFinder
    {
        private readonly Document _doc;
        private readonly AbstractTransactionBuilder _transactionBuilder;
        private readonly double _elbowRadius;
        private readonly MEPSystemModel _sourceMEPModel;

        /// <summary>
        /// Instantiate an object to find path.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="transactionBuilder"></param>
        /// <param name="elbowRadius"></param>
        /// <param name="sourceMEPModel"></param>
        public IvanovPathFinder(Document doc, AbstractTransactionBuilder transactionBuilder,
            double elbowRadius, MEPSystemModel sourceMEPModel)
        {
            _doc = doc;
            _transactionBuilder = transactionBuilder;
            _elbowRadius = elbowRadius;
            _sourceMEPModel = sourceMEPModel;
        }


        /// <inheritdoc/>
        public List<XYZ> Find(XYZ point1, XYZ point2)
        {
            var exceptions = ExceptionElements.Select(obj => obj.IntegerValue).ToList();
            var options = new FinderOptions(exceptions)
            {
                ElbowWidth = _elbowRadius
            };

            //класс анализирует геометрию
            //Task<List<XYZ>> pathTask = null;
            MEPCurve baseCurveForPath = _sourceMEPModel.Root.BaseElement as MEPCurve;
            GeometryDocuments geometryDocuments = null;

            PathFinderToOnePoint finder = null;
            _transactionBuilder.BuildRevitTask(() =>
            {
                geometryDocuments = GeometryDocuments.Create(_doc, options);
                (double width, double heigth) = MEPCurveUtils.GetWidthHeight(baseCurveForPath);
                //класс для поиска пути
                finder = new PathFinderToOnePoint(point1, point2,
                             width, heigth, geometryDocuments, options);
            }, "pathFind").Wait();

            //ищем путь
            Task<List<XYZ>> pathTask = finder.FindPath(new CancellationTokenSource().Token);
            pathTask.Wait();
            List<XYZ> path = pathTask.Result;

            //объединяем прямые последовательные участки пути в один сегмент
            path = Optimizer.MergeStraightSections(path, options);

            return path;
        }

        /// <inheritdoc/>
        public List<ElementId> ExceptionElements { get; set; }
    }
}
