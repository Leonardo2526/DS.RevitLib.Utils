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
            //GetMEPSystemComponents(Composite);

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

        //private void GetMEPSystemComponents(Component composite)
        //{

        //    if (composite.Root is not null)
        //    {
        //        MEPSystemComponent mep = composite.Root as MEPSystemComponent;
        //        MEPSystemComponents.Add(mep);
        //    }

        //    foreach (Component child in composite.Children)
        //    {
        //        GetMEPSystemComponents(child);
        //    }

        //}


        public List<Element> GetElements(Composite composite)
        {
            List<Element> list = new List<Element>();

            MEPSystemComponent rootMEPComp = composite.Root as MEPSystemComponent;
            list.AddRange(rootMEPComp.Elements);

            List<MEPSystemComponent> childrenMEPComp = composite.Children.Select(x => x as MEPSystemComponent).ToList();
            List<Element> children = childrenMEPComp.SelectMany(x => x.Elements).ToList();
            list.AddRange(children);


            List<MEPSystemComponent> parentsMEPComp = composite.Parents.Select(x => x as MEPSystemComponent).ToList();
            List<Element> parents = parentsMEPComp.SelectMany(x => x.Elements).ToList();
            list.AddRange(parents);

            return list;
        }

    }
}
