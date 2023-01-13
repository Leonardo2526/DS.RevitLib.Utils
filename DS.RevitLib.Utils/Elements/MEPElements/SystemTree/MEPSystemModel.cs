using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

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


        #region Methods

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

        #endregion


    }
}
