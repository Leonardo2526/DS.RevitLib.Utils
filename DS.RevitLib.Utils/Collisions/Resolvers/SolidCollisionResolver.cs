using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Collisions.Resolve;
using DS.RevitLib.Utils.Collisions.Search;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    public class SolidCollisionResolver : AbstractCollisionResolver
    {

        private readonly Solid _intersectionSolid;
        private readonly SolidModelExt _solidModel;
        private readonly MEPCurve _targetElement;
        private readonly List<SolidElemCollision> _collisions;

        public SolidCollisionResolver(SolidCollisionChecker collisionChecker, List<SolidElemCollision> collisions, 
            TransformModel transformModel, SolidModelExt solidModel, MEPCurve targetElement) :
            base(collisionChecker)
        {
            _collisions = collisions;
            TransformModel = transformModel;
            _solidModel = solidModel;
            _targetElement = targetElement;
        }

        public TransformModel TransformModel { get; private set; }


        public override void Resolve()
        {
            var aclr = new AroundCenterLineRotateResolver(_collisionChecker, _solidModel, TransformModel);
            var clr = new RotateCenterLineResolver(_collisionChecker);

            //try resolve by rotation around center line
            aclr.SetSuccessor(clr); // if not resolved, rotate center line at 180 degeres.
            clr.SetSuccessor(aclr); // if not resolved, rotate around center line.
            aclr.Resolve();
            IsResolved = aclr.IsResolved;

            //try resolve in next available point
            //while (!IsResolved)
            //{
            //    IsResolved = ResolveInPoint(_placementPoint);
            //}
        }
    }
}
