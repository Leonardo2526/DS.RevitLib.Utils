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
            Document doc = collision.item1.Document.IsLinked ?
              collision.item2.Document : collision.item1.Document;

            var solid1 = collision.item1.Document.IsLinked ?
                collision.item1.GetTransformed(collision.item1.GetLink(doc)) :
                collision.item1.Solid();

            var solid2 = collision.item2.Document.IsLinked ?
           collision.item2.GetTransformed(collision.item2.GetLink(doc)) :
           collision.item2.Solid();

            return Solids.SolidUtils.GetIntersection(solid1, solid2, minVolume);
        }

        public static Solid GetIntersectionSolid(this (Solid item1, Element item2) collision, Document doc, double minVolume = 0)
        {
            var solid2 = collision.item2.Document.IsLinked ?
         collision.item2.GetTransformed(collision.item2.GetLink(doc)) :
         collision.item2.Solid();

            return Solids.SolidUtils.
                        GetIntersection(collision.item1, solid2, minVolume);
        }

        public static Solid GetIntersectionSolidWithInsulation(this (Element item1, Element item2) collision, double minVolume = 0)
        {
            var intersectionSolid = collision.GetIntersectionSolid(minVolume);

            var solids = new List<Solid>()
            {intersectionSolid};

            Document doc = collision.item1.Document.IsLinked ? 
                collision.item2.Document : collision.item1.Document;

            Solid rInsSolid = null;
            var rIns = collision.item1.GetInsulation();
            if (rIns is not null && rIns.IsValidObject)
            { rInsSolid = rIns.Document.IsLinked ?
                rIns.GetTransformed(rIns.GetLink(doc)) :
                rIns.Solid();}

            Solid sInsSolid= null;
            var sIns = collision.item2.GetInsulation();
            if (sIns is not null && sIns.IsValidObject)
            { sInsSolid = sIns.Document.IsLinked ?
                sIns.GetTransformed(sIns.GetLink(doc)) :
                sIns.Solid();
            }

            Solid insulationIntersectionSolid = null;
            if (rInsSolid != null && sInsSolid != null)
            {
                insulationIntersectionSolid = Solids.SolidUtils.
                        GetIntersection(rInsSolid, sInsSolid, minVolume);
            }
            else if(rInsSolid != null && sInsSolid == null)
            {
                insulationIntersectionSolid = Solids.SolidUtils.
                      GetIntersection(rInsSolid, collision.item2.Solid(), minVolume);
            }
            else if(sInsSolid == null && sInsSolid != null)
            {
                insulationIntersectionSolid = Solids.SolidUtils.
                     GetIntersection(collision.item1.Solid(), sInsSolid, minVolume);
            }

            if (insulationIntersectionSolid is not null)
            { solids.Add(insulationIntersectionSolid); }

            return Solids.SolidUtils.UniteSolids(solids);
        }

       
    }
}
