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
        private readonly CancellationToken _cancellationToken;
        private readonly double _offset;

        /// <summary>
        /// Instantiate an object to find path.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elbowRadius"></param>
        /// <param name="sourceMEPModel"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="offset">Offset from element</param>
        /// <param name="transactionBuilder"></param>
        public IvanovPathFinder(Document doc, double elbowRadius, MEPSystemModel sourceMEPModel, CancellationToken cancellationToken, double offset =0,
            AbstractTransactionBuilder transactionBuilder = null)
        {
            _doc = doc;
            _elbowRadius = elbowRadius;
            _sourceMEPModel = sourceMEPModel;
            _cancellationToken = cancellationToken;
            _offset = offset;
            _transactionBuilder = transactionBuilder;
        }


        /// <inheritdoc/>
        public List<XYZ> Find(XYZ point1, XYZ point2)
        {
            var excludedElements = ExceptionElements.Select(obj => obj.IntegerValue).ToList();

            var excludedElementsInsulationIds = new List<ElementId>();
            ExceptionElements.ForEach(obj =>
            {
                Element insulation = InsulationLiningBase.GetInsulationIds(_doc, obj)?
                  .Select(x => _doc.GetElement(x)).FirstOrDefault();
                if (insulation != null && insulation.IsValidObject) { excludedElementsInsulationIds.Add(insulation.Id); }
            });
            excludedElements.AddRange(excludedElementsInsulationIds.Select(obj => obj.IntegerValue).ToList());

            var mainOptions = new MainFinderOptions(excludedElements);

            var secondaryOptions = new SecondaryOptions()
            {
                ElbowWidth = _elbowRadius,
                x_y_coef = 1,
                z_coef = 1
            };
          
            //класс анализирует геометрию
            //Task<List<XYZ>> pathTask = null;
            MEPCurve baseCurveForPath = _sourceMEPModel.Root.BaseElement as MEPCurve;
            GeometryDocuments geometryDocuments = null;

            PathFinderToOnePointDefault finder = null;
            _transactionBuilder.BuildRevitTask(() =>
            {
                geometryDocuments = GeometryDocuments.Create(_doc, mainOptions);
                geometryDocuments.UnsubscribeDocumentChangedEvent();

            }, "create GeometryDocuments").Wait();
            (double width, double heigth) = MEPCurveUtils.GetWidthHeight(baseCurveForPath);

            //класс для поиска пути
            finder = new PathFinderToOnePointDefault(point1, point2,
                         heigth, width, _offset, _offset, geometryDocuments, mainOptions, secondaryOptions);

            //ищем путь
            Task<List<XYZ>> pathTask = finder.FindPath(_cancellationToken);
            pathTask.Wait();
            List<XYZ> path = pathTask.Result;

            //объединяем прямые последовательные участки пути в один сегмент
            path = Optimizer.MergeStraightSections(path, mainOptions);

            return path;
        }

        /// <inheritdoc/>
        public List<ElementId> ExceptionElements { get; set; }
    }
}
