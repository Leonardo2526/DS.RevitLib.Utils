using System;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.MEP.SystemTree
{
    /// <summary>
    /// The 'Component' abstract class
    /// </summary>
    public abstract class Component
    {
        public virtual void AddChild(Component c)
        {
            throw new NotImplementedException();
        }

        public virtual void AddParent(Component c)
        {
            throw new NotImplementedException();
        }

        public virtual void AddChildren(List<Component> list)
        {
            throw new NotImplementedException();
        }

        public virtual void AddParents(List<Component> list)
        {
            throw new NotImplementedException();
        }

        //public virtual void Remove(Component c) { }
        //public virtual void Display(int depth) { }
    }
}
