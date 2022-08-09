using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    public class NodeElement
    {
        public NodeElement(Element element, Relation relation)
        {
            this.Element = element;
            Relation = relation;
        }

        public Element Element { get; }
        public Relation Relation { get; }
    }
}
