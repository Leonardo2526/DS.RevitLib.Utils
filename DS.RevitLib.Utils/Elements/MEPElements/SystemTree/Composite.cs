using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    public class Composite : Component
    {
        public Composite(Component root)
        {
            Root = root;
        }

        public Composite()
        { }

        public Component Root { get; set; }
        public List<Component> Children { get; } = new List<Component>();
        public List<Component> Parents { get; } = new List<Component>();

        private readonly List<Component> _components = new();

        /// <summary>
        /// All components in current object.
        /// </summary>
        public List<Component> Components
        {
            get 
            {
                if (_components.Any()) { return _components; }

                _components.Add(Root);
                foreach (var component in Children) { _components.Add(component); }
                foreach (var component in Parents) { _components.Add(component); }
                return _components; 
            }
        }


        /// <summary>
        /// Add component ro Children
        /// </summary>
        /// <param name="c"></param>
        public override void AddChild(Component c)
        {
            Children.Add(c);
        }

        /// <summary>
        /// Add component ro Parent
        /// </summary>
        /// <param name="c"></param>
        public override void AddParent(Component c)
        {
            Parents.Add(c);
        }

        /// <summary>
        /// Add list of components to Children
        /// </summary>
        /// <param name="list"></param>
        public override void AddChildren(List<Component> list)
        {
            Children.AddRange(list);
        }

        /// <summary>
        /// Add list of components to Parents
        /// </summary>
        /// <param name="list"></param>
        public override void AddParents(List<Component> list)
        {
            Parents.AddRange(list);
        }      

    }
}
