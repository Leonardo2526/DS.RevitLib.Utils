using Autodesk.Revit.DB;
using DS.RevitLib.Test.Collisions.Resolvers;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids;
using DS.RevitLib.Utils.Solids.Models;
using DS.RevitLib.Utils.Visualisators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.ElementTransferTest
{
    public class ModelTransformer
    {
        private readonly TargetMEPCuve _targetMEPCurve;
        private SolidModelExt _operationModel;
        private readonly SolidCollisionChecker _collisionChecker;

        public ModelTransformer(TargetMEPCuve targetMEPCuve, SolidModelExt opertationModel, SolidCollisionChecker collisionChecker)
        {
            _targetMEPCurve = targetMEPCuve;
            _operationModel = opertationModel;
            _collisionChecker = collisionChecker;
        }

        public void Create()
        {
            //Place and align solid in point
            var targetModel = new SolidModelExt(_targetMEPCurve.MEPCurve);
            targetModel.Basis = new Basis(targetModel.Basis.X, targetModel.Basis.Y, targetModel.Basis.Z, _targetMEPCurve.StartPlacementPoint);
            var transformModel = new TransformBuilder(_operationModel.Basis, targetModel.Basis).Build();
            _operationModel.Transform(transformModel.Transforms);
            //Show(_operationModel);

            var checkedObjects1 = new List<SolidModelExt>() { _operationModel };
            var collisions = _collisionChecker.GetCollisions(checkedObjects1);
            if (!collisions.Any())
            {
                return;
            }

            //Search available position for solid
            var solidElemCollisions = collisions.Cast<SolidElemCollision>().ToList();
            var solidCollisionClient = new SolidCollisionClient(solidElemCollisions, _collisionChecker, _targetMEPCurve);
            solidCollisionClient.Resolve();
        }
    }
}
