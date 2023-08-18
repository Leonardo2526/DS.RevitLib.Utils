using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Collisions.Detectors
{
    public interface ISolidCollisionDetector : ICollisionDetector
    {
        List<ICollision> GetCollisions(Solid checkObject1, List<Element> checkObjects2ToExclude = null);
    }
}