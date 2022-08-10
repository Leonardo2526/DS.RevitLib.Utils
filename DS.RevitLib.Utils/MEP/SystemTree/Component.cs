using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    /// <summary>
    /// The 'Component' abstract class
    /// </summary>
    public abstract class Component
    {
        public virtual void Add(Component c)
        {
            throw new NotImplementedException();
        }
        //public virtual void Remove(Component c) { }
        //public virtual void Display(int depth) { }
    }
}
