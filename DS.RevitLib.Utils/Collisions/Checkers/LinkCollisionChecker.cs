using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

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

        public override List<ICollision> GetCollisions()
        {
            throw new NotImplementedException();
        }

        public override List<ICollision> GetCollisions(List<SolidModelExt> checkedObjects1)
        {
            List<ICollision> collisions = new List<ICollision>();

            Options options = new Options();
            GeometryElement geo1 = _revitLinkInstance.get_Geometry(options);

            var linkSolidsExt = new List<SolidModelExt>();
            foreach (var elem in CheckedObjects2)
            {
                linkSolidsExt.Add(new SolidModelExt(elem));
            }

            //var solids = geo1.Where(o => o is Solid);
            var trans = _revitLinkInstance.GetTotalTransform();

            foreach (var obj1 in checkedObjects1)
            {
                foreach (var linkObj in linkSolidsExt)
                {
                    Solid transformedSolid = SolidUtils.CreateTransformed(linkObj.Solid, trans);
                    var intersectionSolidsResult = BooleanOperationsUtils.
                        ExecuteBooleanOperation(obj1.Solid, transformedSolid, BooleanOperationsType.Intersect);
                    if (intersectionSolidsResult.Volume > 0)
                    {
                        var col = BuildCollision(obj1, linkObj.Element);
                        collisions.Add(col);
                    }
                }
            }

            return collisions;
        }

        protected override ICollision BuildCollision(SolidModelExt object1, Element object2)
        {
            return new SolidElemCollision(object1, object2);
        }
    }
}
