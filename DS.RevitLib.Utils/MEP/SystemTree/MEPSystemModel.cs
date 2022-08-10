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
        public Composite Composite { get; set; }

        public MEPSystemModel(Composite rootComponent)
        {
            Composite = rootComponent;
            GetMEPSystemComponents(Composite);

        }

        public List<MEPSystemComponent> MEPSystemComponents { get; private set; } = new List<MEPSystemComponent>();

        public List<MEPSystemComponent> ParentComponents
        {
            get
            {
                return GetParentComponents(Composite);
            }
        }

        public List<Element> ParentElements
        {
            get
            {
                return ParentComponents.SelectMany(x => x.Elements).ToList();
            }
        }
        public List<Element> AllElements
        {
            get
            {
                return MEPSystemComponents.SelectMany(x => x.Elements).ToList();
            }
        }


        private List<MEPSystemComponent> GetParentComponents(Composite composite)
        {
            List<MEPSystemComponent> list = new List<MEPSystemComponent>();

            foreach (Composite child in composite.Children)
            {
                MEPSystemComponent mep = child.Root as MEPSystemComponent;
                list.Add(mep);
            }

            return list;
        }

        private void GetMEPSystemComponents(Composite composite)
        {         

            if (composite.Root is not null)
            {
                MEPSystemComponent mep = composite.Root as MEPSystemComponent;
                MEPSystemComponents.Add(mep);
            }

            foreach (Composite child in composite.Children)
            {
                GetMEPSystemComponents(child);
            }

        }
    }
}
