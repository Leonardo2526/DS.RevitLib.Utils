using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Various.Bases;

namespace DS.RevitLib.Utils.Extensions
{
    public interface ISolidOffsetExtractor
    {
        Solid Extract(XYZ startPoint, XYZ endPoint, BasisXYZ targetBasis);
    }
}