using Autodesk.Revit.DB;
using DS.RevitLib.Test.ElementTransferTest;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Collisions.Resolvers;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DS.RevitLib.Test.Collisions.Resolvers
{
    public class SolidCollisionClient
    {
        private readonly List<SolidElemCollision> _collisions;
        private readonly SolidCollisionChecker _collisionChecker;
        private readonly TargetMEPCuve _targetMEPCuve;
        private XYZ _currentPoint;

        public SolidCollisionClient(List<SolidElemCollision> collisions, SolidCollisionChecker collisionChecker, TargetMEPCuve targetMEPCuve)
        {
            _collisions = collisions;
            _collisionChecker = collisionChecker;
            _targetMEPCuve = targetMEPCuve;
            _currentPoint = _targetMEPCuve.StartPlacementPoint;
        }


        public void Resolve()
        {
            var collision = _collisions.First();

            var aclr = new AroundCenterLineRotateResolver(collision, _collisionChecker);
            var clr = new RotateCenterLineResolver(collision, _collisionChecker);

            aclr.SetSuccessor(clr); // if not resolved, rotate center line at 180 degeres.
            clr.SetSuccessor(aclr); // if not resolved, rotate around center line.
            aclr.Resolve();

            if (!aclr.IsResolved)
            {
                Solid totalIntersectionSolid = GetIntersectionSolid(_collisions);
                var mr = new MoveResolver(collision, _collisionChecker, _currentPoint, _targetMEPCuve, totalIntersectionSolid);
                aclr = new AroundCenterLineRotateResolver(collision, _collisionChecker);
                clr = new RotateCenterLineResolver(collision, _collisionChecker);

                mr.SetSuccessor(aclr);     
                aclr.SetSuccessor(clr); // if not resolved, rotate center line at 180 degeres.
                clr.SetSuccessor(aclr); // if not resolved, rotate around center line.
                mr.Resolve();
                _currentPoint = mr.MovePoint;
                if (_currentPoint is null)
                {
                    return;
                }
            }
        }

        private Solid GetIntersectionSolid(List<SolidElemCollision> collisions)
        {
            var solid = collisions.Select(obj => obj.GetIntersection()).ToList();
            return DS.RevitLib.Utils.Solids.SolidUtils.UniteSolids(solid);
        }
    }
}
