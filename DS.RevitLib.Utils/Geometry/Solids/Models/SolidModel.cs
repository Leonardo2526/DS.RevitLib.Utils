using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Solids.Models
{

    public class SolidModel : AbstractSolidModel
    {
        public SolidModel(Solid solid)
        {
            Solid = solid;
        }

       /// <summary>
       /// Add solid to current object.
       /// </summary>
       /// <param name="solid"></param>
        public void AddSolid(Solid solid)
        {
            var newSollids = new List<Solid>() { Solid, solid };
            Solid = SolidUtils.UniteSolids(newSollids);
        }
    }
}
