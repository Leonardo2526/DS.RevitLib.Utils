using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Solids.Models;

namespace DS.RevitLib.Utils.Collisions.Models
{
    public class SolidElemTransformCollision : Collision<SolidModelExt, Element>
    {
        public SolidElemTransformCollision(SolidModelExt object1, Element object2) : base(object1, object2)
        {
        }

        public Solid GetIntersection()
        {
            Solid solid1 = Object1.Solid;
            if (Transform1 is not null)
            {
                solid1 = SolidUtils.CreateTransformed(solid1, Transform1);
            }

            Solid solid2 = ElementUtils.GetSolid(Object2);
            if (Transform2 is not null)
            {
                solid2 = SolidUtils.CreateTransformed(solid2, Transform2);
            }

            return DS.RevitLib.Utils.Solids.SolidUtils.GetIntersection(solid1, solid2);
        }

        public Transform Transform1 { get; set; }
        public Transform Transform2 { get; set; }
    }
}
