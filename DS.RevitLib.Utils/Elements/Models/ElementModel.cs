using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Models;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Solids.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Elements.Models
{
    public class ElementModel : AbstractElementModel
    {
        public ElementModel(Element element, SolidModel solidModel) : base(element, solidModel)
        {
        }


        public override double GetSizeByVector(XYZ orth)
        {
            throw new NotImplementedException();
        }
    }
}
