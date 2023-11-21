using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions.Models
{
    public class DummyCollision : IMEPCollision
    {
        public int Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public CollisionStatus Status => throw new NotImplementedException();

        public MEPCurve Item1 => throw new NotImplementedException();

        public Element Item2 => throw new NotImplementedException();

        public void Show()
        {
            throw new NotImplementedException();
        }
    }
}
