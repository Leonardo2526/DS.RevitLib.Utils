using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Extensions
{
    public static class CollisionExtensions
    {

        public static Solid GetIntersectionSolid(this (Element item1, Element item2) collision, double minVolume = 0)
        {            
            return Solids.SolidUtils.
                        GetIntersection(collision.item1.Solid(), collision.item2.Solid(), minVolume);
        }

        public static Solid GetIntersectionSolid(this (Solid item1, Element item2) collision, double minVolume = 0)
        {
            return Solids.SolidUtils.
                        GetIntersection(collision.item1, collision.item2.Solid(), minVolume);
        }

        public static Solid GetIntersectionSolidWithInsulation(this (Element item1, Element item2) collision, double minVolume = 0)
        {
            var intersectionSolid = collision.GetIntersectionSolid(minVolume);

            var solids = new List<Solid>()
            {intersectionSolid};

            var rIns = collision.item1.GetInsulation();
            if (rIns is not null && rIns.IsValidObject)
            { solids.Add(rIns.Solid()); }

            var sIns = collision.item2.GetInsulation();
            if (sIns is not null && sIns.IsValidObject)
            { solids.Add(sIns.Solid()); }

            return Solids.SolidUtils.UniteSolids(solids);
        }

       
    }
}
