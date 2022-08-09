using Autodesk.Revit.DB;
using DS.RevitLib.Utils.MEP.Creator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    public class MEPSystemModel
    {
        public Component RootComponent { get; set; }

        public MEPSystemModel(Component rootComponent)
        {
            RootComponent = rootComponent;
        }

        //public List<MEPSystemComponent> MEPSystemComponents { get; private set; } = new List<MEPSystemComponent>();
        //public List<Element> AllElements
        //{
        //    get
        //    {
        //        return MEPSystemComponents.SelectMany(x => x.Elements).ToList();
        //    }
        //}
    }
}
