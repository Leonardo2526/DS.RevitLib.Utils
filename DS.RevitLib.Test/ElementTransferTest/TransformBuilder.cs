using DS.RevitLib.Test.Collisions.Resolvers;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Test.ElementTransferTest
{
    public class TransformBuilder
    {
        private readonly TargetMEPCuve _targetMEPCurve;
        private SolidModelExt _operationModel;
        private readonly SolidCollisionChecker _collisionChecker;

        public TransformBuilder(TargetMEPCuve targetMEPCuve, SolidModelExt opertationModel, SolidCollisionChecker collisionChecker)
        {
            _targetMEPCurve = targetMEPCuve;
            _operationModel = opertationModel;
            _collisionChecker = collisionChecker;
        }

        public TransformModel Build()
        {
            //Solid place
            var solidPlacer = new SolidPlacer(_operationModel, _targetMEPCurve._mEPCurve, _targetMEPCurve.StartPlacementPoint);
            _operationModel = solidPlacer.Place();

            var checkedObjects1 = new List<SolidModelExt>() { _operationModel };
            var collisions = _collisionChecker.GetCollisions(checkedObjects1);
            if (!collisions.Any())
            {
                return _operationModel.TransformModel;
            }

            var solidElemCollisions = collisions.Cast<SolidElemCollision>().ToList();
            SolidElemCollision currentCollision = solidElemCollisions.First();


            var solidCollisionClient = new SolidCollisionClient(solidElemCollisions, _collisionChecker, _targetMEPCurve);
            solidCollisionClient.Resolve();

            return _operationModel.TransformModel;
        }
    }
}
