using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;

using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Solids.Models;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements.Transfer.Resolvers
{
    internal class RotateCenterLineResolver : CollisionResolver
    {

        public RotateCenterLineResolver(SolidModelExt _operationElement, (Solid, Element) collision, 
            IElementCollisionDetector detector, List<Element> excludedElements) :
            base(_operationElement ,collision, detector, excludedElements)
        {}

        public override void Resolve()
        {
            XYZ axis = _operationElement.Basis.Y;
            double angle = 180.DegToRad();

            Transform rotateTransform = Transform.CreateRotationAtPoint(axis, angle, _operationElement.CentralPoint);
            _operationElement.Transform(rotateTransform);

            UnresolvedCollisions = _detector.GetCollisions(_operationElement.Solid);

            if (!UnresolvedCollisions.Any())
            {
                IsResolved = true;
            }
            else
            {
                _successor?.Resolve();
                if (_successor?.UnresolvedCollisions is not null && _successor.UnresolvedCollisions.Any())
                {
                    UnresolvedCollisions = _successor.UnresolvedCollisions;
                }
            }
        }
    }
}
