using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DS.RevitLib.Utils.Collisions.Models
{
    public class SolidElemCollision : AbstractCollision<Solid, Element>
    {
        public SolidElemCollision(Solid object1, List<Element> collisionObjects) : base(object1, collisionObjects)
        {
        }

        public override object GetIntersection()
        {
            var elementsIntersections = new Dictionary<Element, Solid>();

            foreach (var element in CollisionObjects)
            {
                Solid elemSolid = ElementUtils.GetSolid(element);
                Solid intersectionSolid = DS.RevitLib.Utils.Solids.SolidUtils.GetIntersection(BaseObject, elemSolid);
                elementsIntersections.Add(element, intersectionSolid);
            }

            return elementsIntersections;
        }
    }
}
