using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Collisions.Checkers
{
    public class LinkCollisionChecker : CollisionChecker<SolidModelExt, Element>, ICollisionChecker
    {
        RevitLinkInstance _revitLinkInstance;

        public LinkCollisionChecker(List<Element> checkedObjects2, RevitLinkInstance revitLinkInstance, List<Element> exludedObjects = null) :
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
                return CheckedObjects1.First().Element.Document;
            }
        }

        public override List<ICollision> GetCollisions()
        {
            List<ICollision> collisions = new List<ICollision>();
            foreach (var obj1 in CheckedObjects1)
            {
                collisions.AddRange(GetObjectCollisions(obj1));
            }

            return collisions;
        }

        private List<ICollision> GetObjectCollisions(SolidModelExt object1)
        {
            List<ICollision> collisions = new List<ICollision>();

            foreach (var linkObj in LinkSolidsExt)
            {
                var intersectionSolidsResult = BooleanOperationsUtils.
                    ExecuteBooleanOperation(object1.Solid, linkObj.Solid, BooleanOperationsType.Intersect);
                if (intersectionSolidsResult.Volume > 0)
                {
                    var col = BuildCollision(object1, linkObj.Element);
                    collisions.Add(col);
                }
            }

            return collisions;
        }

        public override List<ICollision> GetCollisions(List<SolidModelExt> checkedObjects1)
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

        protected override ICollision BuildCollision(SolidModelExt object1, Element object2)
        {
            var col = new SolidElemCollision(object1, object2);
            col.Transform2 = _revitLinkInstance.GetTotalTransform();
            return col;
        }
    }
}
