using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Solids.Models;

namespace DS.RevitLib.Utils.Collisions.Models
{
    public class SolidElemCollision : Collision<SolidModelExt, Element>
    {
        public SolidElemCollision(SolidModelExt object1, Element object2) : base(object1, object2)
        {
        }

        public Solid GetIntersection()
        {
            Solid elemSolid = ElementUtils.GetSolid(Object2);
            return DS.RevitLib.Utils.Solids.SolidUtils.GetIntersection(Object1.Solid, elemSolid);
        }
    }
}
