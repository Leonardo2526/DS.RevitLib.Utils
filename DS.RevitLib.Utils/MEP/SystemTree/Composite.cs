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
        public Component Root { get; set; }
        public List<Component> Children { get; } = new List<Component>();

        /// <summary>
        /// Add component ro Children
        /// </summary>
        /// <param name="c"></param>
        public override void Add(Component c)
        {
            Children.Add(c);
        }

        /// <summary>
        /// Add list of components to Children
        /// </summary>
        /// <param name="list"></param>
        public override void AddRange(List<Component> list)
        {
            Children.AddRange(list);
        }
    }
}
