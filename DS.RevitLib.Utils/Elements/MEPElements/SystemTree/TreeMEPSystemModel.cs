using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    public class TreeMEPSystemModel
    {
        public Composite Composite { get; set; }

        public TreeMEPSystemModel(Composite rootComponent)
        {
            Composite = rootComponent;

        }

        public List<MEPSystemComponent> MEPSystemComponents
        {
            get
            {
                return GetAllComponents();
            }
        }

        public List<MEPSystemComponent> ParentComponents
        {
            get
            {
                return GetParentComponents(Composite);
            }
        }

        public List<MEPSystemComponent> ChildComponents
        {
            get
            {
                return GetChildComponents(Composite);
            }
        }

        public List<Element> ParentElements
        {
            get
            {
                return ParentComponents.SelectMany(x => x.Elements).ToList();
            }
        }

        public List<Element> ChildElements
        {
            get
            {
                return ChildComponents.SelectMany(x => x.Elements).ToList();
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

            foreach (Composite child in composite.Parents)
            {
                MEPSystemComponent mep = child.Root as MEPSystemComponent;
                list.Add(mep);
            }

            return list;
        }

        private List<MEPSystemComponent> GetChildComponents(Composite composite)
        {
            List<MEPSystemComponent> list = new List<MEPSystemComponent>();

            foreach (Composite child in composite.Children)
            {
                MEPSystemComponent mep = child.Root as MEPSystemComponent;
                list.Add(mep);
            }

            return list;
        }


        private List<MEPSystemComponent> GetAllComponents()
        {
            var mEPSystemComponents = new List<MEPSystemComponent>();
            mEPSystemComponents.AddRange(ParentComponents);
            mEPSystemComponents.AddRange(ChildComponents);

            return mEPSystemComponents;
        }


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

        public List<Element> GetRootElements(Composite composite)
        {
            List<Element> list = new List<Element>();

            MEPSystemComponent rootMEPComp = composite.Root as MEPSystemComponent;
            list.AddRange(rootMEPComp.Elements);

            return list;
        }

    }
}
