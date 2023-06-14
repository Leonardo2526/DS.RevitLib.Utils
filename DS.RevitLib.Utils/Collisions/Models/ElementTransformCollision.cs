using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Models
{
    public class ElementTransformCollision : Collision<Element, Element>
    {
        public ElementTransformCollision(Element object1, Element object2) : base(object1, object2)
        {
        }

        public Transform Transform1 { get; set; }
        public Transform Transform2 { get; set; }
    }
}
