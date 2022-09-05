using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Collisions.Checkers
{
    public interface ICollisionChecker
    {
        public List<ICollision> GetCollisions();
    }
}
