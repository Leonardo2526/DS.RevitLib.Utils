using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Solids.Models;
using System;

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
