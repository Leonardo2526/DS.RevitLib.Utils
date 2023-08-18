using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Various;
using MoreLinq;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    public class MEPSystemModel
    {
        private readonly List<MEPSystemComponent> _allComponents;

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
                if (_allComponents == null) { return GetAllComponents(); }
                else { return _allComponents; }
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
                return AllComponents.SelectMany(x => x.Elements).Where(x => x.IsValidObject).ToList().
                    DistinctBy(obj => obj.Id).ToList();
            }
        }

        /// <summary>
        /// Update <see cref="Root"/> component in current <see cref="MEPSystemModel"/>.
        /// </summary>
        /// <returns>Returns updated <see cref="Root"/> component.</returns>
        public MEPSystemComponent UpdateRoot()
        {
            return (MEPSystemComponent)(Composite.Root = new ComponentBuilder(Root.BaseElement).Build());
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

        /// <summary>
        /// Get all components that contains <paramref name="element"/>.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Return empty list if no components contains <paramref name="element"/>.</returns>
        public List<MEPSystemComponent> GetComponents(Element element)
        {
            var components = new List<MEPSystemComponent>();
            AllComponents.ForEach(c =>
            {
                var ids = c.Elements.Select(e => e.Id).ToList();
                if (ids.Any() && ids.Contains(element.Id))
                { components.Add(c); }
            });

            return components;
        }

        /// <summary>
        /// Find root element by any <paramref name="element"/> in current system.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="currentComponent"></param>
        /// <returns>Returns closest root element to <paramref name="element"/>.</returns>
        public Element FindRootElem(Element element, MEPSystemComponent currentComponent = null)
        {

            if (Root.Elements.Select(obj => obj.Id).Contains(element.Id)) { return element; }

            var components = GetComponents(element).Where(c => !c.Equals(currentComponent));
            foreach (var comp in components)
            {
                var nodes = comp.ParentNodes.Concat(comp.ChildrenNodes);
                foreach (var node in nodes)
                {
                    if (FindRootElem(node.Element, comp) is not null) { return node.Element; }
                }
            }

            return null;
        }


        /// <summary>
        /// Get elements span of root component.
        /// </summary>
        /// <param name="element1">Any element in system.</param>
        /// <param name="element2">Any element in system.</param>
        /// <param name="includeEdge">Check if include or not edge root elements to result.</param>
        /// <returns>Returns list of elements from edge root elements found by <paramref name="element1"/> and <paramref name="element2"/>.</returns>
        public List<Element> GetRootElements(Element element1, Element element2, bool includeEdge = true)
        {
            var rootElem1 = FindRootElem(element1);
            var rootElem2 = FindRootElem(element2);

            return Root.GetElements(rootElem1, rootElem2, includeEdge);
        }

        /// <summary>
        /// Select all elements in document.
        /// </summary>
        /// <param name="uiDoc"></param>
        public void SelectAll(UIDocument uiDoc)
        {
            uiDoc.Selection.SetElementIds(AllElements.Select(obj => obj.Id).ToList());
        }

        /// <summary>
        /// Select all elements in document.
        /// </summary>
        /// <param name="uiDoc"></param>
        public void DeselectAll(UIDocument uiDoc)
        {
            uiDoc.Selection.ClearSelection2();
        }

        #endregion


    }
}
