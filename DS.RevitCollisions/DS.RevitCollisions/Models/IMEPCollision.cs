using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.Models;

namespace DS.RevitCollisions.Models
{
    public interface IMEPCollision : IElementCollision, ICollision
    {
        Basis Basis { get; }
        MEPCurveModel ResolvingModel { get; }
    }
     
}