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

        public override void Add(Component c)
        {
            Children.Add(c);
        }

        public override void AddRange(List<Component> list)
        {
            Children.AddRange(list);
        }
    }
}
