using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Elements.Models;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Transforms;
using DS.RevitLib.Utils.Elements.Transfer.AvailableModels;
using DS.RevitLib.Utils.Elements.Transfer.TransformModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DS.RevitLib.Utils.Collisions.Detectors;

namespace DS.RevitLib.Utils.Elements.Transfer.TransformBuilders
{
    public class FamToLineMultipleBuilder
    {
        private readonly double _minFamInstLength = 50.mmToFyt2();
        private readonly double _minCurveLength;
        private readonly ISolidCollisionDetector _detector;
        private readonly List<Element> _excludedElements;
        private readonly double _minPlacementLength;
        private AvailableLineService _lineService;
        private double _currentPlacementLength;
        private List<XYZ> _path;
        private MEPCurveModel _mEPCurveModel;

        public FamToLineMultipleBuilder(double minCurveLength, ISolidCollisionDetector detector, List<Element> excludedElements)
        {
            _minCurveLength = minCurveLength;
            _detector = detector;
            _excludedElements = excludedElements;
            _minPlacementLength = _minFamInstLength + 2 * minCurveLength;
        }

        private double GetFamInsLength(Element fam)
        {
            (Connector con1, Connector con2) = ConnectorUtils.GetMainConnectors(fam);
            return con1.Origin.DistanceTo(con2.Origin);
        }


        protected TransformModel GetModel(object operationObject, object targetObject)
        {
            var operation = (SolidModelExt)operationObject;
            var target = (LineModel)targetObject;
            var builder = new FamToLineTransformBuilder(operation, target,
                  _detector, _currentPlacementLength, _path, _mEPCurveModel, _minCurveLength, _excludedElements);
            var model = builder.Build();

            var (line1, line2) =
                target.Line.Cut(operation.ConnectorsPoints.First(), operation.ConnectorsPoints.Last(), out Line cuttedLine);


            Line maxLine = line1.ApproximateLength > line2.ApproximateLength ? line1 : line2;

            var lineModel = new LineModel(maxLine, target.Basis);

            //add splitted mEPCurve to stack
            if (_lineService.CheckMinPlacementLength(lineModel))
            {
                _lineService.AvailableCurves.AddToFront(lineModel);
            }

            return model as FamToLineTransformModel;
        }


        public List<TransformModel> Build(List<SolidModelExt> sourceObjects, List<LineModel> targetObjects,
            List<XYZ> path, AbstractElementModel startModel)
        {
            _path = path;
            _mEPCurveModel = startModel as MEPCurveModel;

            _lineService = new AvailableLineService(targetObjects, _minCurveLength, _minPlacementLength);
            if (_lineService.AvailableCurves is null || !_lineService.AvailableCurves.Any())
            {
                string errors = $"No available MEPCurves exist for family insatances placement.";
                Debug.Write(errors) ;
                return null;
            }

            var transforms = new List<TransformModel>();
            foreach (var sObj in sourceObjects)
            {
                var operationObj = sObj.Clone();
                _currentPlacementLength = GetFamInsLength(sObj.Element) + 2 * _minCurveLength;
                LineModel lineModel = _lineService.Get(_currentPlacementLength);
                if (lineModel is null)
                {
                    string errors = $"No available MEPCurves exist for family insatance id ({sObj.Element.Id}) placement.";
                    Debug.WriteLine(errors) ;
                    return null;
                }

                var model = GetModel(operationObj, lineModel);
                transforms.Add(model);
            }

            return transforms;
        }
    }
}
