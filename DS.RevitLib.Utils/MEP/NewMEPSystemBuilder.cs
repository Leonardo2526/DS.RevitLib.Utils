using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.SystemTree.Relatives;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    public class NewMEPSystemBuilder
    {
        private readonly Element _element;

        public NewMEPSystemBuilder(Element element)
        {
            this._element = element;
        }

        public MEPSystemModel Build()
        {
            var comp = new CompositeBuilder(_element);
            var composite = comp.Build();
       
            return new MEPSystemModel(composite);
        }
    }
}
