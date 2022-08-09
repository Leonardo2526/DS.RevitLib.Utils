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
        public NodeElement(FamilyInstance element, Relation relation = Relation.Default)
        {
            this.Element = element;
            SystemRelation = relation;
        }

        public FamilyInstance Element { get; }
        public Relation SystemRelation { get; set; }
    }
}
