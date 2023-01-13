using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    internal class NodePusher : Pusher
    {
        public NodePusher(NodeElement node, Element element, ComponentBuilder componentBuilder) : base(node, element, componentBuilder)
        {
        }

        public override void Push()
        {
            _componentBuilder.Stack.Push(_node.Element);

            Relation relation = GetRelation(_node.Element, _element);
            _node.SystemRelation = relation;
            _componentBuilder.Nodes.Add(_node);
        }
    }



}
