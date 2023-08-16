using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.Collisions.Checkers;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Solids.Models;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Elements.Transfer.Resolvers
{
    public abstract class CollisionResolver
    {
        protected CollisionResolver _successor;
        protected readonly SolidModelExt _operationElement;
        protected readonly ISolidCollisionDetector _detector;
        protected readonly List<Element> _excludedElements;

        protected CollisionResolver(SolidModelExt operationElement, ICollision collision, ISolidCollisionDetector detector, List<Element> excludedElements)
        {
            _operationElement = operationElement;
            Collision = collision;
            _detector = detector;
            _excludedElements = excludedElements;
        }

        public ICollision Collision { get; }
        public bool IsResolved { get; protected set; }
        public List<ICollision> UnresolvedCollisions { get; protected set; }


        public abstract void Resolve();
        public void SetSuccessor(CollisionResolver successor)
        {
            _successor = successor;
        }
    }
}
