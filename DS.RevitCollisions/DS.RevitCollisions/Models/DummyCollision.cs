using Autodesk.Revit.DB;
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
        public Basis Basis => throw new NotImplementedException();

        public MEPCurveModel ResolvingModel => throw new NotImplementedException();

        public CollisionStatus Status => throw new NotImplementedException();

        public Element ResolvingElem => throw new NotImplementedException();

        public Element StateElem => throw new NotImplementedException();

        public void Show()
        {
            throw new NotImplementedException();
        }
    }
}
