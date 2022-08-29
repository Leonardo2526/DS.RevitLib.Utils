using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolvers
{
    public class SolidCollisionResolver : CollisionResolver<TransformModel>
    {
        private readonly TransformModel _transformModel;

        public SolidCollisionResolver(Collision<SolidModelExt, Element> collision, ICollisionChecker collisionChecker, TransformModel transformModel) : 
            base(collision, collisionChecker)
        {
            _transformModel = transformModel;
        }


        public override TransformModel Resolve()
        {
            //resolve by rotation
            var collision = Collision as Collision<SolidModelExt, Element>;
            var aclr = new AroundCenterLineRotateResolver(collision, _collisionChecker, _transformModel);
            var clr = new RotateCenterLineResolver(collision, _collisionChecker, _transformModel);

            //try resolve by rotation around center line
            aclr.SetSuccessor(clr); // if not resolved, rotate center line at 180 degeres.
            clr.SetSuccessor(aclr); // if not resolved, rotate around center line.
            aclr.Resolve();
            Solution = aclr.Solution;

            //try resolve in next available point
            //while (!IsResolved)
            //{
            //    IsResolved = ResolveInPoint(_placementPoint);
            //}

            return Solution;
        }

        //private bool ResolveInPoint(XYZ point)
        //{
        //    CollisionResolver mr = new MoveResolver();
        //    CollisionResolver aclr = new AroundCenterLineRotateResolver();
        //    CollisionResolver clr = new RotateCenterLineResolver();

        //    mr.SetSuccessor(aclr);
        //    aclr.SetSuccessor(clr);
        //    clr.SetSuccessor(aclr);
        //    mr.Resolve();

        //    return mr.IsResolved;
        //}
    }
}
