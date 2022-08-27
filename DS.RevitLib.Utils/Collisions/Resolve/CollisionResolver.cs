using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Resolve
{
    public abstract class CollisionResolver
    {

        protected CollisionResolver _successor;

        public void SetSuccessor (CollisionResolver successor)
        {
            _successor = successor;
        }

        public bool IsResolved { get; protected set; }

        public abstract void Resolve();
        public abstract bool CheckCollisions();
    }
}
