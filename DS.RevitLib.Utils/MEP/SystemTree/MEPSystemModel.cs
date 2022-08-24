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

        public MEPSystemModel(Composite composite)
        {
            Composite = composite;

        }

        public MEPSystemComponent Root
        {
            get
            {
                return Composite.Root as MEPSystemComponent;
            }
        }

        public List<MEPSystemComponent> AllComponents
        {
            get
            {
                return GetAllComponents();
            }
        }

        public List<MEPSystemComponent> Parents
        {
            get
            {
                return GetParents();
            }
        }

        public List<MEPSystemComponent> Childs
        {
            get
            {
                return GetChildren();
            }
        }

        public List<Element> AllElements
        {
            get
            {
                return AllComponents.SelectMany(x => x.Elements).ToList();
            }
        }


        private List<MEPSystemComponent> GetParents()
        {
            List<MEPSystemComponent> list = new List<MEPSystemComponent>();

            foreach (Component item in Composite.Parents)
            {
                var mep = item as MEPSystemComponent;
                list.Add(mep);
            }

            return list;
        }

        private List<MEPSystemComponent> GetChildren()
        {
            List<MEPSystemComponent> list = new List<MEPSystemComponent>();

            foreach (Component item in Composite.Children)
            {
                var mep = item as MEPSystemComponent;
                list.Add(mep);
            }

            return list;
        }


        private List<MEPSystemComponent> GetAllComponents()
        {
            var mEPSystemComponents = new List<MEPSystemComponent>();
            mEPSystemComponents.AddRange(Parents);
            mEPSystemComponents.AddRange(Childs);
            mEPSystemComponents.Add(Root);

            return mEPSystemComponents;
        }


        //public List<Element> GetAllElements(Composite composite)
        //{
        //    List<Element> list = new List<Element>();

        //    MEPSystemComponent rootMEPComp = composite.Root as MEPSystemComponent;
        //    list.AddRange(rootMEPComp.Elements);

        //    List<MEPSystemComponent> childrenMEPComp = composite.Children.Select(x => x as MEPSystemComponent).ToList();
        //    List<Element> children = childrenMEPComp.SelectMany(x => x.Elements).ToList();
        //    list.AddRange(children);


        //    List<MEPSystemComponent> parentsMEPComp = composite.Parents.Select(x => x as MEPSystemComponent).ToList();
        //    List<Element> parents = parentsMEPComp.SelectMany(x => x.Elements).ToList();
        //    list.AddRange(parents);

        //    return list;
        //}

        //public List<Element> GetRootElements(Composite composite)
        //{
        //    List<Element> list = new List<Element>();

        //    MEPSystemComponent rootMEPComp = composite.Root as MEPSystemComponent;
        //    list.AddRange(rootMEPComp.Elements);

        //    return list;
        //}

    }
}
