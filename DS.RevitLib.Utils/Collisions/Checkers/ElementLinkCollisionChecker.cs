using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Collisions.Checkers
{
    public class ElementLinkCollisionChecker : CollisionChecker<Element, Element>, ICollisionChecker
    {
        RevitLinkInstance _revitLinkInstance;

        public ElementLinkCollisionChecker(List<Element> checkedObjects2, RevitLinkInstance revitLinkInstance, List<Element> exludedObjects = null) :
            base(checkedObjects2, exludedObjects)
        {
            _revitLinkInstance = revitLinkInstance;
        }

        protected override FilteredElementCollector Collector { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override ExclusionFilter ExclusionFilter => throw new NotImplementedException();

        public Transform Transform2 { get; private set; }

        private List<SolidModelExt> LinkSolidsExt { get; set; } = new List<SolidModelExt>();

        protected override Document Document
        {
            get
            {
                return CheckedObjects1.First().Document;
            }
        }

        public override List<ICollision> GetCollisions()
        {
            if (CheckedObjects1 is null | CheckedObjects2 is null |
           !CheckedObjects1.Any() | !CheckedObjects2.Any()) return new List<ICollision>();

            List<ICollision> collisions = new List<ICollision>();
            foreach (var obj1 in CheckedObjects1)
            {
                collisions.AddRange(GetObjectCollisions(obj1));
            }

            return collisions;
        }

        private List<ICollision> GetObjectCollisions(Element object1)
        {
            List<ICollision> collisions = new List<ICollision>();

            foreach (var linkObj in LinkSolidsExt)
            {
                var solid = ElementUtils.GetSolid(object1);
                var intersectionSolidsResult = BooleanOperationsUtils.
                    ExecuteBooleanOperation(solid, linkObj.Solid, BooleanOperationsType.Intersect);
                if (intersectionSolidsResult.Volume > 0)
                {
                    var col = BuildCollision(object1, linkObj.Element);
                    collisions.Add(col);
                }
            }

            return collisions;
        }

        public override List<ICollision> GetCollisions(List<Element> checkedObjects1)
        {
            CheckedObjects1 = checkedObjects1;
            List<ICollision> collisions = new List<ICollision>();

            Options options = new Options();
            GeometryElement geo1 = _revitLinkInstance.get_Geometry(options);
            Transform2 = _revitLinkInstance.GetTotalTransform();

            foreach (var elem in CheckedObjects2)
            {
                var model = new SolidModelExt(elem);
                if (Transform2 is not null)
                {
                    model.Transform(Transform2);
                }
                LinkSolidsExt.Add(model);
            }

            //var solids = geo1.Where(o => o is Solid);

            foreach (var obj1 in checkedObjects1)
            {
                collisions.AddRange(GetObjectCollisions(obj1));
            }

            return collisions;
        }

        protected override ICollision BuildCollision(Element object1, Element object2)
        {
            var col = new ElementTransformCollision(object1, object2);
            col.Transform2 = _revitLinkInstance.GetTotalTransform();
            return col;
        }
    }
}
