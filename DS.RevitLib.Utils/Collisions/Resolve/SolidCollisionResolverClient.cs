using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolve
{
    internal class SolidCollisionResolverClient
    {
        private readonly XYZ _placementPoint;
        private readonly Solid _intersectionSolid;
        private readonly SolidModelExt _solidModel;
        private readonly List<Element> _checkedObjects;
        private readonly MEPCurve _targetElement;

        public TransformModel TransformModel { get; private set; }
        public bool _isResolved;

        public void Resolve()
        {
            CollisionResolver aclr = new AroundCenterLineRotateResolver();
            CollisionResolver clr = new RotateCenterLineResolver();

            //try resolve by rotation around center line
            aclr.SetSuccessor(clr); // if not resolved, rotate center line at 180 degeres.
            clr.SetSuccessor(aclr); // if not resolved, rotate around center line.
            aclr.Resolve();
            if (aclr.IsResolved)
            {
                return;
            }

            //try resolve in next available point
            while (!_isResolved)
            {
                _isResolved = ResolveInPoint(_placementPoint);
            }
        }

        private bool ResolveInPoint(XYZ point)
        {
            CollisionResolver mr = new MoveResolver();
            CollisionResolver aclr = new AroundCenterLineRotateResolver();
            CollisionResolver clr = new RotateCenterLineResolver();

            mr.SetSuccessor(aclr);
            aclr.SetSuccessor(clr);
            clr.SetSuccessor(aclr);
            mr.Resolve();

            return mr.IsResolved;
        }
    }
}
