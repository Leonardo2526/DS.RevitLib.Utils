using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;

namespace DS.RevitLib.Utils.Collisions.Models
{
    /// <inheritdoc/>
    public class ElementCollision : Collision<Element, Element>
    {
        private Solid _intersectionSolid;

        /// <inheritdoc/>
        public ElementCollision(Element object1, Element object2) : base(object1, object2)
        {
        }

        /// <summary>
        /// Minimum intersection volume in <see cref="Autodesk.Revit.DB.DisplayUnitType.DUT_CUBIC_CENTIMETERS"/>.
        /// </summary>
        public double MinVolume { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Solid IntersectionSolid
        {
            get
            {
                if (_intersectionSolid == null)
                {
                    _intersectionSolid = Solids.SolidUtils.
                        GetIntersection(ElementUtils.GetSolid(Object1), ElementUtils.GetSolid(Object2), MinVolume);
                }
                return _intersectionSolid;
            }
        }
    }
}
