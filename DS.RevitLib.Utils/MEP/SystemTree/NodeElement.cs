using Autodesk.Revit.DB;

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
        public Element RelationElement { get; set; }
        public Relation SystemRelation { get; set; }
    }
}
