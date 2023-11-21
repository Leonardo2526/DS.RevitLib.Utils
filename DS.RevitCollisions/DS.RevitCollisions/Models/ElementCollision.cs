using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions.Models
{
    internal class ElementCollision : Collision
    {
        public ElementCollision(AbstractElementModel stateElementModel, AbstractElementModel resolvingElementModel, Solid collisionSolid) : 
            base(stateElementModel, resolvingElementModel, collisionSolid)
        {
        }

        public override bool IsValid => throw new NotImplementedException();

        public override bool HaveIntersection => throw new NotImplementedException();

        public override Collision SwapElements()
        {
            throw new NotImplementedException();
        }
    }
}
