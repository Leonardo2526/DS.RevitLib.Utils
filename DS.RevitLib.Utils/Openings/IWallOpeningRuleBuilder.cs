using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Openings
{
    public interface IWallOpeningRuleBuilder 
    {
        Func<(Solid, Element), bool> GetRule(Wall wall);
    }
}
