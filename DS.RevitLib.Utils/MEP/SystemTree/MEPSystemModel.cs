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
        public Composite RootComponent { get; set; }

        public MEPSystemModel(Composite rootComponent)
        {
            RootComponent = rootComponent;
        }

        public List<MEPSystemComponent> MEPSystemComponents
        {
            get
            {
                return GetMEPSystemComponents();
            }
        }
        public List<Element> AllElements
        {
            get
            {
                return MEPSystemComponents.SelectMany(x => x.Elements).ToList();
            }
        }


        private List<MEPSystemComponent> GetMEPSystemComponents()
        {
            List<MEPSystemComponent> list = new List<MEPSystemComponent>();
            foreach (var comp in RootComponent.Children)
            {
                var mepComp = comp as MEPSystemComponent;
                list.Add(mepComp);
            }

            return list;
        }
    }
}
