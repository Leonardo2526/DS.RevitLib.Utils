﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.Transactions;
using DS.RevitLib.Utils.Various;
using PathFinderLib;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.PathCreators
{
    /// <summary>
    /// An object that represents finder to get path.
    /// </summary>
    public class PathFindCreator : IPathCreator
    {
        private  Document _doc;
        private  AbstractTransactionBuilder _transactionBuilder;
        private  double _elbowRadius;
        private CancellationToken _cancellationToken;
        private XYZ _xVector;
        private double _offset;
        private double _width;
        private double _height;

        /// <summary>
        /// Instantiate an object to find path.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elbowRadius"></param>
        /// <param name="xVector">Align vector for path in XY plane.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="offset">Offset from element</param>
        /// <param name="transactionBuilder"></param>
        public PathFindCreator Create(Document doc, double elbowRadius, XYZ xVector, CancellationToken cancellationToken, double width, double height, double offset = 0,
            AbstractTransactionBuilder transactionBuilder = null)
        {
            _doc = doc;
            _elbowRadius = elbowRadius;
            _cancellationToken = cancellationToken;
            _xVector = xVector;
            _offset = offset;
            _width = width;
            _height = height;
            _transactionBuilder = transactionBuilder;
            return this;
        }

        /// <inheritdoc/>
        public async Task<List<XYZ>> CreateAsync(XYZ point1, XYZ point2)
        {
            var excludedElements = ExceptionElements.Select(obj => obj.IntegerValue).ToList();

            var excludedElementsInsulationIds = new List<ElementId>();
            ExceptionElements.ForEach(obj =>
            {
                if (_doc.GetElement(obj) is Pipe || _doc.GetElement(obj) is Duct)
                {
                    Element insulation = InsulationLiningBase.GetInsulationIds(_doc, obj)?
                  .Select(x => _doc.GetElement(x)).FirstOrDefault();
                if (insulation != null && insulation.IsValidObject) { excludedElementsInsulationIds.Add(insulation.Id); }
                }
            });
            excludedElements.AddRange(excludedElementsInsulationIds.Select(obj => obj.IntegerValue).ToList());

            var mainOptions = new MainFinderOptions(excludedElements);
            var secondaryOptions = new SecondaryOptions()
            {
                ElbowWidth = _elbowRadius,
                x_y_coef = 1,
                z_coef = 1,
                XVector = _xVector
            };
          
            //класс анализирует геометрию
            GeometryDocuments geometryDocuments = null;

            PathFinderToOnePointDefault finder = null;

            var action = () =>
            {
                _transactionBuilder.Build(() =>
                {
                    geometryDocuments = GeometryDocuments.Create(_doc, mainOptions);
                    geometryDocuments.UnsubscribeDocumentChangedEvent();
                }, "create GeometryDocuments");
            };
            if (_doc.IsRevitContext()) { action(); }
            else { await RevitTask.RunAsync(() => action()); }

            //класс для поиска пути
            finder = new PathFinderToOnePointDefault(point1, point2,
                         _height, _width, _offset, _offset, geometryDocuments, mainOptions, secondaryOptions);

            //ищем путь
            List<XYZ> path = await finder.FindPath(_cancellationToken);

            //объединяем прямые последовательные участки пути в один сегмент
            path = Optimizer.MergeStraightSections(path, mainOptions);

            var zigzag = new ZigZagCleaner(path, mainOptions, secondaryOptions);
            var cleanPath = zigzag.Clear(geometryDocuments, _height, _width, _offset, _offset);

            return cleanPath;
        }

        /// <inheritdoc/>
        public List<ElementId> ExceptionElements { get; set; }
    }
}
