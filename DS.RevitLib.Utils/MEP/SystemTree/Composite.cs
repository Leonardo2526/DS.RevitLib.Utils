using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    public class Composite : Component
    {
        public Composite(Component root)
        {
            Root = root;
        }

        public Composite()
        {}

        public Component Root { get; set; }
        public List<Component> Children { get; } = new List<Component>();
        public List<Component> Parents { get; } = new List<Component>();

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
