using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.Solids.Models
{

    public class SolidModel : AbstractSolidModel
    {
        public SolidModel(Solid solid)
        {
            Solid = solid;
        }

        public override AbstractSolidModel Clone()
        {
            throw new System.NotImplementedException();
        }
    }
}
