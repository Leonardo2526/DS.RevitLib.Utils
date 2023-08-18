using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Transforms;
using DS.RevitLib.Utils.Elements.Transfer.TransformModels;
using Revit.Async;
using System.Collections.Generic;
using DS.RevitLib.Utils.Collisions.Detectors;

namespace DS.RevitLib.Utils.Elements.Transfer.TransformBuilders
{
    internal class FamToLineTransformBuilder : TransformBuilder
    {
        private readonly ISolidCollisionDetector _detector;
        private readonly double _placementLength;
        private readonly List<XYZ> _points;
        private readonly MEPCurveModel _mEPCurveModel;
        private readonly double _minCurveLength;
        private readonly List<Element> _excludedElements;
        private readonly SolidModelExt _operationObject;

        public FamToLineTransformBuilder(SolidModelExt sourceObject, LineModel targetObject,
            ISolidCollisionDetector detector, double placementLength, List<XYZ> points,
            MEPCurveModel mEPCurveModel, double minCurveLength, List<Element> excludedElements) :
            base(sourceObject, targetObject)
        {
            _detector = detector;
            _placementLength = placementLength;
            _points = points;
            _mEPCurveModel = mEPCurveModel;
            _minCurveLength = minCurveLength;
            _excludedElements = excludedElements;
            _operationObject = sourceObject.Clone();
        }

        public override TransformModel Build()
        {
            var target = _targetObject as LineModel;
            var source = _sourceObject as SolidModelExt;

            TargetPlacementModel targetModel = new TargetModelBuilder(target, _placementLength, _points).Build();

            var finder = new PositionFinder(targetModel, source, _detector, _mEPCurveModel, _minCurveLength, _excludedElements);
            finder.Find();
            //DocModel.TransactionBuiler.BuildRevitTask(() =>
            //{
            //    source.Basis.Show(DocModel.Doc);
            //    target.Basis.Show(DocModel.Doc);
            //    _operationObject.Basis.Show(DocModel.Doc);
            //}, "show Basis").Wait();
            //RevitTask.RunAsync(() => DocModel.UiDoc.RefreshActiveView()).Wait();

            var transformModel = new BasisTransformBuilder(_operationObject.Basis, source.Basis).Build() as BasisTransformModel;
            var model = new FamToLineTransformModel(source, target);
            model.Transforms = transformModel.Transforms;
            model.MoveVector = transformModel.MoveVector;
            model.Rotations = transformModel.Rotations;

            return model;
        }
    }
}
