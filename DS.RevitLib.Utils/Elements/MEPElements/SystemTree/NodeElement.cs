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

        /// <summary>
        /// Tee or spud element of node.
        /// </summary>
        public FamilyInstance Element { get; }

        /// <summary>
        /// Element connected to tee or spud of node.
        /// </summary>
        public Element RelationElement { get; set; }


        public Relation SystemRelation { get; set; }
    }
}
