using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.Collisions.Models
{
    /// <inheritdoc/>
    public class SolidElementCollision : Collision<Solid, Element>
    {
        private Solid _intersectionSolid;

        /// <inheritdoc/>
        public SolidElementCollision(Solid object1, Element object2) : base(object1, object2)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public Solid IntersectionSolid
        {
            get { 
                if(_intersectionSolid == null )
                {
                    _intersectionSolid = Solids.SolidUtils.
                        GetIntersection(Object1, ElementUtils.GetSolid(Object2));
                }
                return _intersectionSolid; 
            }
        }

    }
}
