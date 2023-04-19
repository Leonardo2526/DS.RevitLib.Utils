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
        /// Minimum intersection volume in <see cref="Autodesk.Revit.DB.DisplayUnitType.DUT_CUBIC_CENTIMETERS"/>.
        /// </summary>
        public double MinVolume { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public Solid IntersectionSolid
        {
            get { 
                if(_intersectionSolid == null )
                {
                    _intersectionSolid = Solids.SolidUtils.
                        GetIntersection(Object1, ElementUtils.GetSolid(Object2), MinVolume);
                }
                return _intersectionSolid; 
            }
        }

    }
}
