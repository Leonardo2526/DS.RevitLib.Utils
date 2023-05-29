using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Elements.Transfer;
using DS.RevitLib.Utils.Elements.Transfer.TransformBuilders;
using DS.RevitLib.Utils.Elements.Transfer.TransformModels;
using DS.RevitLib.Utils.FamilyInstances;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.TransactionCommitter;
using DS.RevitLib.Utils.Transforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Elements.MEPElements.Transfer
{
    /// <summary>
    /// An object that represents factory to transfer <see cref="Autodesk.Revit.DB.FamilyInstance"/>'s to <see cref="MEPSystemModel"/>.
    /// </summary>
    public class FamilyInstTransferFactory : IFamilyInstTransferFactory
    {

        public FamilyInstTransferFactory Build()
        {
            var collisionCheckers = GetCollisionCheckers(_collision.ResolvingElem, boxXYZ);
            _famToLineBuilder = _mainSettings.TransferElementsToPath ?
                new FamToLineMultipleBuilder(_traceSettings.D, collisionCheckers, _collision) : null;

            var transformModels = GetTransforms(_point1, _point2, path);
        }


        /// <inheritdoc/>
        public List<FamilyInstance> Transfer(List<FamilyInstance> source, MEPSystemModel target)
        {
            Debug.WriteLine("Trying to transform families...");
            //transform elements
            DocModel.TransactionBuiler.Build(() => TransformFamilies(famLineModels), "TransformFamilies.");
            DocModel.TransactionBuiler.Build(() => InsertFamilies(mEPCurves, famLineModels), "InsertFamilies.");
            Debug.WriteLine("Families transformation completed.");
        }

        private List<ICollisionChecker> GetCollisionCheckers(Element baseElement, BoundingBoxXYZ boxXYZ)
        {
            var collisionCheckers = new List<ICollisionChecker>();

            var excludedObjects = new List<Element> { baseElement };
            excludedObjects.AddRange(excludedObjects);
            excludedObjects.AddRange(_elementsToDelete);

            var elementsInOutlineIds = GetElementsInBB(boxXYZ, excludedObjects).Select(obj => obj.Id);
            if (!elementsInOutlineIds.Any()) { return collisionCheckers; }

            //Get model checker
            List<Element> modelElements = DocModel.GetInstance().DocElements.
                Where(obj => elementsInOutlineIds.Contains(obj.Id)).ToList();
            collisionCheckers.Add(new SolidCollisionChecker(modelElements, excludedObjects));

            //get link checkers
            foreach (var link in DocModel.GetInstance().LinksElements)
            {
                collisionCheckers.Add(new LinkCollisionChecker(link.Value, link.Key, null));
            }

            return collisionCheckers;
        }

        private List<TransformModel> BuildTransforms(List<XYZ> path)
        {
            var lineModelBuilder = new LineModelBuilder(path, _point1, _elbowRadius, _elementsToDelete);
            var lineModels = lineModelBuilder.Build();
            _baseMEPCurveModel = lineModelBuilder.BaseMEPCurveModel;

            var allAccIds = _sourceMEPSystemModel.Root.Accessories.Select(a => a.Id);
            var accecoriesSpan = _elementsToDelete.Where(obj => allAccIds.Contains(obj.Id)).OfType<FamilyInstance>().ToList();

            var accecoriesExt = accecoriesSpan.Select(obj => new SolidModelExt(obj)).ToList();
            if (!accecoriesExt.Any()) { return new List<TransformModel>(); };

            var transformModels = _famToLineBuilder?.
                Build(accecoriesExt, lineModels, path, lineModelBuilder.BaseMEPCurveModel);

            //FamInstTransformAvailable = _transfromBuilder.Available;
            return transformModels;
        }

        private List<TransformModel> GetTransforms(IConnectionPoint p1, IConnectionPoint p2, List<XYZ> path)
        {
            var elements = _sourceMEPSystemModel.GetRootElements(_point1.Element, _point2.Element);
            var allAccIds = _sourceMEPSystemModel.Root.Accessories.Select(a => a.Id);
            var famInstances = elements.OfType<FamilyInstance>().
                Where(obj => obj.Id != _point1.Element.Id && obj.Id != _point2.Element.Id).
                Where(obj => allAccIds.Contains(obj.Id)).ToList();

            if (!famInstances.Any())
            {
                var curve = _sourceMEPSystemModel.Root.BaseElement as MEPCurve;
                _baseMEPCurveModel = new MEPCurveModel(curve, new SolidModel(ElementUtils.GetSolid(curve)));
                return new List<TransformModel>();
            }
            else
            {
                return BuildTransforms(path);
            }
        }

        private void InsertFamilies(List<MEPCurve> mEPCurves, List<FamToLineTransformModel> famLineModels)
        {
            var creator = new FamInstTransactions(DocModel.Doc, new RollBackCommitter(), MainSettings.TransactionPrefix, false);
            foreach (var model in famLineModels)
            {
                var elem = model.SourceObject as SolidModelExt;
                var fam = elem.Element as FamilyInstance;
                MEPCurve mc = fam.GetMEPCuveToInsert(mEPCurves);

                if (mc is null)
                { Debug.WriteLine($"Failed to insert familyInstance {fam.Id}", TraceLevel.Error); continue; }

                creator.Insert(fam, mc, out List<MEPCurve> splittedMEPCurves);

                //update mEPCurves list.
                mEPCurves.AddRange(splittedMEPCurves);
                mEPCurves = mEPCurves.Distinct().ToList();
            }
        }

        private void TransformFamilies(List<FamToLineTransformModel> famLineModels)
        {
            famLineModels?.ForEach(model =>
            {
                TokenModel.RequestToken(_collision.InnerTokenSource.Token);
                var elem = model.SourceObject as SolidModelExt;
                MEPElementUtils.Disconnect(elem.Element);
                TransformElement(elem.Element, model.MoveVector, model.Rotations);
            });
        }
    }
}
