using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;

using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Elements.Transfer;
using DS.RevitLib.Utils.Elements.Transfer.TransformBuilders;
using DS.RevitLib.Utils.Elements.Transfer.TransformModels;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.TransactionCommitter;
using DS.RevitLib.Utils.Transforms;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DS.RevitLib.Utils.Elements.MEPElements.Transfer
{
    /// <summary>
    /// An object that represents factory to transfer <see cref="Autodesk.Revit.DB.FamilyInstance"/>'s to <see cref="MEPSystemModel"/>.
    /// </summary>
    public class FamilyInstTransferFactory : IFamilyInstTransferFactory
    {
        private readonly Document _doc;
        private readonly IElementCollisionDetector _detector;
        private readonly List<XYZ> _path;
        private readonly MEPSystemModel _sourceModel;
        private readonly ITraceSettings _traceSettings;
        private readonly double _elbowRadius;
        private readonly List<Element> _excludedElements;
        private FamToLineMultipleBuilder _famToLineBuilder;

        /// <summary>
        /// Instantiate an object that represents factory to transfer <see cref="Autodesk.Revit.DB.FamilyInstance"/>'s to <see cref="MEPSystemModel"/>.
        /// </summary>
        public FamilyInstTransferFactory(IElementCollisionDetector detector, List<XYZ> path, MEPSystemModel sourceModel,
            ITraceSettings traceSettings, double elbowRadius, List<Element> excludedElements)
        {
            _doc = sourceModel.Root.BaseElement.Document;
            _detector = detector;
            _path = path;
            _sourceModel = sourceModel;
            _traceSettings = traceSettings;
            _elbowRadius = elbowRadius;
            _excludedElements = excludedElements;
        }

        /// <inheritdoc/>
        public List<FamilyInstance> Transfer(List<FamilyInstance> source, MEPSystemModel target)
        {
            if (source is null || source.Count == 0) { return source; }

            Debug.WriteLine("Trying to transform families...");

            _famToLineBuilder = new FamToLineMultipleBuilder(_traceSettings.D, _detector, _excludedElements);

            List<TransformModel> transformModels = BuildTransforms(_path, source);
            var famLineModels = transformModels?.Cast<FamToLineTransformModel>().ToList();

            if(famLineModels is null || famLineModels.Count == 0) 
            { 
                Debug.WriteLine("Failed to get famLineModels to complete elements transfrom.", TraceLevel.Error);
                return new List<FamilyInstance>(); 
            }

            //transform elements
            TransformFamilies(famLineModels);
            var insertedFamilies = InsertFamilies(target.Root.MEPCurves, famLineModels);

            Debug.WriteLine("Families transformation completed.");

            return insertedFamilies;
        }




        private List<TransformModel> BuildTransforms(List<XYZ> path, List<FamilyInstance> source)
        {
            MEPCurve startMEPCurve = null;
            foreach (var fm in source)
            {
                startMEPCurve = ConnectorUtils.GetConnectedElements(fm).FirstOrDefault(obj => obj is MEPCurve) as MEPCurve;
                if (startMEPCurve != null) { break; }
            }
            if(startMEPCurve == null) { Debug.WriteLine("Filed to find startMEPCurve to build transfer factory.", TraceLevel.Error); return null; }

            var lineModelBuilder = new LineModelBuilder(startMEPCurve, path, _elbowRadius);
            var lineModels = lineModelBuilder.Build();

            var accecoriesExt = source.Select(obj => new SolidModelExt(obj)).ToList();
            if (!accecoriesExt.Any()) { return new List<TransformModel>(); };

            var transformModels = _famToLineBuilder?.
                Build(accecoriesExt, lineModels, path, lineModelBuilder.BaseMEPCurveModel);

            return transformModels;
        }

        private List<FamilyInstance> InsertFamilies(List<MEPCurve> mEPCurves, List<FamToLineTransformModel> famLineModels)
        {
            var insertedFamilies = new List<FamilyInstance>();

            var creator = new FamInstTransactions(_doc, new RollBackCommitter());
            foreach (var model in famLineModels)
            {
                var elem = model.SourceObject as SolidModelExt;
                var fam = elem.Element as FamilyInstance;
                MEPCurve mc = fam.GetMEPCuveToInsert(mEPCurves);

                if (mc is null)
                { Debug.WriteLine($"Failed to insert familyInstance {fam.Id}", TraceLevel.Error); continue; }

                var inseredFamily = creator.Insert(fam, mc, out List<MEPCurve> splittedMEPCurves);
                if (inseredFamily != null) { insertedFamilies.Add(inseredFamily); }

                //update mEPCurves list.
                mEPCurves.AddRange(splittedMEPCurves);
                mEPCurves = mEPCurves.Distinct().ToList();
            }

            return insertedFamilies;
        }

        private void TransformFamilies(List<FamToLineTransformModel> famLineModels)
        {
            famLineModels?.ForEach(model =>
            {
                var elem = model.SourceObject as SolidModelExt;
                MEPElementUtils.Disconnect(elem.Element);
                TransformElement(elem.Element, model.MoveVector, model.Rotations);
            });
        }
        private Element TransformElement(Element element, XYZ moveVector, List<RotationModel> rotationModels)
        {
            if (moveVector is not null)
            {
                ElementTransformUtils.MoveElement(_doc, element.Id, moveVector);
            }
            foreach (var rot in rotationModels)
            {
                ElementTransformUtils.RotateElement(_doc, element.Id, rot.Axis, rot.Angle);
            }
            return element;
        }
    }
}
