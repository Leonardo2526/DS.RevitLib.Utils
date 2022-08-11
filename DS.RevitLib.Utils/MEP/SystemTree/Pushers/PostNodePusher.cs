using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.MEP.SystemTree.Relatives
{
    internal class PostNodePusher : Pusher
    {
        public PostNodePusher(NodeElement node, Element element, ComponentBuilder componentBuilder) : 
            base(node, element, componentBuilder)
        {
        }

        public bool PushedToParent { get; private set; }

        public override void Push()
        {
            Relation teeRelation = GetRelation(_node.Element, _element);

            switch (teeRelation)
            {              
                case Relation.Child:
                    if (_node.SystemRelation == Relation.Child)
                    {
                        _componentBuilder.Stack.Push(_element);
                    }
                    else if(_node.SystemRelation == Relation.Parent)
                    {
                        _node.RelationElement = _element;
                        _componentBuilder.Elements.Move(0, _componentBuilder.Elements.Count - 1);
                        PushedToParent = true;
                    }
                    break;
                case Relation.Parent:
                    _node.RelationElement = _element;
                    break;
                default:
                    break;
            }
        }
    }



}
